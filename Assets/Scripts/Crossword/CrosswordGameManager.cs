﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CrosswordGameManager : UsesVirtualKeyboardMiniGameManager
{
    [Header("Spawning")]
    [SerializeField] private Transform parentSpawnedTo;
    [SerializeField] private CrosswordBoard numbersBoardPrefab;
    private CrosswordBoard board;
    private CrosswordBoardCell selectedCell;
    private CrosswordCluePlacementData selectedWord;

    [Header("Audio")]
    [SerializeField] private string onInput = "gm_onInput";

    [Header("Overhead Settings")]
    [SerializeField] private float delayBetweenCellsInRestartSequence;
    [SerializeField] protected float delayOnRestart = 1.0f;

    [Header("Generation Settings")]
    [SerializeField] private Vector2Int boardSize;
    [SerializeField] private Vector2Int minMaxWordSize;
    [SerializeField] private int targetNumWords;
    [SerializeField] private int minNumWords;
    [SerializeField] private int maxAlottedAttemptsPerWord;
    [SerializeField] private int maxRetryAttempts;
    [SerializeField] private int numCluesToParse;
    [SerializeField] private int parseFileIncrement;
    private InputMode currentInputMode;
    private bool forceUpdate;
    private bool gameHasBeenRestarted;
    private bool hasDealtWIthFunctionButtons;
    public bool AllowMove { get; protected set; }

    [Header("Num Tests")]
    [SerializeField] private int numTests = 1;

    [Header("References")]
    [SerializeField] private TextAsset[] clueFiles;
    private int textAssetIndex;
    [SerializeField] protected TextMeshProUGUI timeTakenText;
    [SerializeField] protected TextMeshProUGUI hsTimeTakenText;
    [SerializeField] private TextMeshProUGUI clueText;
    [SerializeField] private ManipulateRectTransformOnMouseInput manipGamePlace;
    [SerializeField] private RectTransform scrollViewRect;
    [SerializeField] private ScreenAnimationController hideEndScreenController;
    private AdditionalFuncVirtualKeyboardButton pencilButton;
    private AdditionalFuncVirtualKeyboardButton spawnToolTipsButton;

    private List<CrosswordClue> clues;
    private List<string> possibleAnswers;
    private Dictionary<int, List<CrosswordClue>> clueDict;
    protected override IEnumerator Setup()
    {
        // Generate the board
        board = Instantiate(numbersBoardPrefab, parentSpawnedTo);

        yield return StartCoroutine(board.Generate(SelectCell, clueDict, clues, possibleAnswers, boardSize,
            minMaxWordSize, targetNumWords, minNumWords, maxAlottedAttemptsPerWord, maxRetryAttempts));

        TransitionManager._Instance.FadeIn();

        yield return StartCoroutine(ShowKeyboard());

        if (gameHasBeenRestarted)
            gameStarted = true;

    }

    protected override IEnumerator Restart()
    {
        TransitionManager._Instance.FadeOut();

        hideEndScreenController.Fade(0);

        if (spawnedToolTips.Count > 0)
        {
            Destroy(spawnedToolTips[0].transform.parent.parent.gameObject);
            spawnedToolTips.Clear();
        }

        StartCoroutine(HideKeyboard());

        clueText.text = "";

        gameHasBeenRestarted = true;
        AllowMove = false;

        yield return StartCoroutine(board.ActOnEachBoardCellWithDelay(cell =>
        {
            StartCoroutine(cell.ChangeScale(0));
            cell.SetInteractable(false);
        }, delayBetweenCellsInRestartSequence, true));

        yield return new WaitForSeconds(delayOnRestart);

        Destroy(board.gameObject);

        AllowMove = true;
    }

    public void RegenerateBoard()
    {
        StartCoroutine(CallRegenerateBoard());
    }

    private IEnumerator CallRegenerateBoard()
    {
        if (spawnedToolTips != null && spawnedToolTips.Count > 0)
        {
            Destroy(spawnedToolTips[0].transform.parent.parent.gameObject);
            spawnedToolTips.Clear();
        }

        clueText.text = "";

        AllowMove = false;

        yield return StartCoroutine(board.ActOnEachBoardCellWithDelay(cell =>
        {
            StartCoroutine(cell.ChangeScale(0));
            cell.SetInteractable(false);
        }, delayBetweenCellsInRestartSequence, true));

        Destroy(board.gameObject);

        AllowMove = true;

        // Generate new board
        board = Instantiate(numbersBoardPrefab, parentSpawnedTo);

        yield return StartCoroutine(board.Generate(SelectCell, clueDict, clues, possibleAnswers, boardSize,
            minMaxWordSize, targetNumWords, minNumWords, maxAlottedAttemptsPerWord, maxRetryAttempts));
    }

    protected override IEnumerator GameWon()
    {
        SetTimerHighScore(hsTimeTakenText);

        timeTakenText.text = "Time to Solve: " + Utils.ParseDuration((int)timer);

        hideEndScreenController.Fade(1);

        yield return null;
    }

    public enum InputMode
    {
        INPUT,
        PENCIL,
    }

    private new void Awake()
    {
        base.Awake();

        textAssetIndex = RandomHelper.RandomIntExclusive(0, clueFiles.Length);
        MathematicallyRandomParse();
    }

    private void ParseByChar()
    {
        // Initialize Data Structs
        clueDict = new Dictionary<int, List<CrosswordClue>>();
        possibleAnswers = new List<string>();
        clues = new List<CrosswordClue>();

        // Populate Clue Dict with Lists
        for (int i = minMaxWordSize.x; i <= minMaxWordSize.y; i++)
        {
            clueDict.Add(i, new List<CrosswordClue>());
        }

        // Parse Data
        string st = clueFiles[textAssetIndex].text;

        int numClues = 0;
        int indexOnLine = 0;
        string[] currentLine = new string[2];
        string currentToken = "";
        for (int i = 0; i < st.Length; i++)
        {
            if (numClues > numCluesToParse)
            {
                break;
            }

            char c = st[i];
            // Debug.Log(c);
            if (c.Equals('\t'))
            {
                // Debug.Log("Tab: " + indexOnLine);
                currentLine[indexOnLine] = currentToken;
                indexOnLine++;
                currentToken = "";

                if (indexOnLine == 2)
                {
                    // Debug.Log("Newline");

                    // Reset
                    currentToken = "";
                    indexOnLine = 0;

                    string answer = currentLine[0];
                    if (answer.Length < minMaxWordSize.x || answer.Length > minMaxWordSize.y)
                    {
                        continue;
                    }

                    possibleAnswers.Add(answer);
                    clueDict[answer.Length].Add(new CrosswordClue(currentLine[1], answer));
                    numClues++;
                }
            }
            else
            {
                if (!c.Equals('\n'))
                    currentToken += c;
            }
        }

        // Shuffle Lists
        foreach (KeyValuePair<int, List<CrosswordClue>> kvp in clueDict)
        {
            kvp.Value.Shuffle();
            clues.AddRange(kvp.Value);
        }

        // Sort Clue List
        CrosswordClue[] arr = clues.ToArray();
        Array.Sort(arr, (y, x) => x.GetAnswer().Length.CompareTo(y.GetAnswer().Length));
        clues = arr.ToList();
        // Debug.Log(clues.Count + ", " + clues[0] + ", " + clues[clues.Count - 1]);
    }

    private void FullRandomParse()
    {
        // Initialize Data Structs
        clueDict = new Dictionary<int, List<CrosswordClue>>();
        possibleAnswers = new List<string>();
        clues = new List<CrosswordClue>();

        // Populate Clue Dict with Lists
        for (int i = minMaxWordSize.x; i <= minMaxWordSize.y; i++)
        {
            clueDict.Add(i, new List<CrosswordClue>());
        }

        // Parse Data
        string st = clueFiles[textAssetIndex].text;
        string[] data = st.Split(new char[] { '\n' });
        List<string> lines = data.ToList();

        int numClues = 0;
        // data is now an array of all lines
        // take numEntries number of lines at random
        while (numClues < numCluesToParse)
        {
            if (lines.Count == 0)
            {
                Debug.Log("Ran Out of Lines to Parse");
                break;
            }
            string s = RandomHelper.GetRandomFromList(lines);
            lines.Remove(s);
            string[] tokens = s.Split('\t');
            string answer = tokens[0];
            if (answer.Length < minMaxWordSize.x || answer.Length > minMaxWordSize.y)
            {
                continue;
            }
            possibleAnswers.Add(answer);
            clueDict[answer.Length].Add(new CrosswordClue(tokens[1], answer));
            numClues++;
        }

        // Shuffle Lists
        foreach (KeyValuePair<int, List<CrosswordClue>> kvp in clueDict)
        {
            kvp.Value.Shuffle();
            clues.AddRange(kvp.Value);
        }

        // Sort Clue List
        CrosswordClue[] arr = clues.ToArray();
        Array.Sort(arr, (y, x) => x.GetAnswer().Length.CompareTo(y.GetAnswer().Length));
        clues = arr.ToList();
        // Debug.Log(clues.Count + ", " + clues[0] + ", " + clues[clues.Count - 1]);
    }

    private void MathematicallyRandomParse()
    {
        // Initialize Data Structs
        clueDict = new Dictionary<int, List<CrosswordClue>>();
        possibleAnswers = new List<string>();
        clues = new List<CrosswordClue>();

        // Populate Clue Dict with Lists
        for (int i = minMaxWordSize.x; i <= minMaxWordSize.y; i++)
        {
            clueDict.Add(i, new List<CrosswordClue>());
        }

        // Parse Data
        string st = clueFiles[textAssetIndex].text;
        string[] data = st.Split(new char[] { '\t', '\n' });

        // Populate Lists
        int start = MathHelper.RoundToNearestGivenInt(RandomHelper.RandomIntExclusive(0, data.Length - (numCluesToParse * parseFileIncrement)), 2);
        int cap = start + (numCluesToParse * parseFileIncrement);
        // Debug.Log("Total: " + data.Length + ", NumEntries: " + numEntries + ", inc: " + inc + ", Start: " + start);
        for (int i = start; i < cap; i += parseFileIncrement)
        {
            string answer = data[i];
            // Debug.Log(answer);
            if (answer.Length < minMaxWordSize.x || answer.Length > minMaxWordSize.y)
            {
                continue;
            }
            // Debug.Log(new CrosswordClue(data[i + 3], data[i + 2]));
            possibleAnswers.Add(answer);
            clueDict[answer.Length].Add(new CrosswordClue(data[i + 1], answer));
            // Debug.Log("Added Answer: " + answer);
            // in case the increment causes indexer to rise above the cap before filling in all the entries
            if (i + parseFileIncrement > cap && possibleAnswers.Count < numCluesToParse)
            {
                i = 0;
            }
        }
        // Debug.Log("Done Parsing Data");

        // Shuffle Lists
        foreach (KeyValuePair<int, List<CrosswordClue>> kvp in clueDict)
        {
            kvp.Value.Shuffle();
            clues.AddRange(kvp.Value);
        }

        // Sort Clue List
        CrosswordClue[] arr = clues.ToArray();
        Array.Sort(arr, (y, x) => x.GetAnswer().Length.CompareTo(y.GetAnswer().Length));
        clues = arr.ToList();
        // Debug.Log(clues.Count + ", " + clues[0] + ", " + clues[clues.Count - 1]);
    }

    private new void Update()
    {
        base.Update();
    }

    private void SelectCell(CrosswordBoardCell cell)
    {
        if (selectedCell)
        {
            board.UnshowCells();
            selectedCell.Deselect();
        }

        // Clicked on a cell that belongs to the current word
        if (selectedWord != null && selectedWord.GetIncorperatedCells().Contains(cell))
        {
            if (cell.GetReservedBy().Count == 1)
            {
                // Clicked on a cell that belongs to the current word and only the current word

                // Selected Cell changes
                // Selected Word does not change
                // Debug.Log("1");
            }
            else if (cell.GetReservedBy().Count == 2)
            {
                CrosswordCluePlacementData wordA = cell.GetReservedBy()[0];
                CrosswordCluePlacementData wordB = cell.GetReservedBy()[1];

                // Clicked on a cell that belongs to the current word as well as another word
                if (cell == selectedWord.GetIncorperatedCells()[0] && cell != selectedCell)
                {
                    // 
                    // Debug.Log("2");
                }
                else if (selectedWord == wordA && cell == wordB.GetIncorperatedCells()[0] && cell != selectedCell)
                {
                    selectedWord = wordB;
                    // Debug.Log("3");
                }
                else if (selectedWord == wordB && cell == wordA.GetIncorperatedCells()[0] && cell != selectedCell)
                {
                    selectedWord = wordA;
                    // Debug.Log("4");
                }
                else
                {
                    // Show the word that is different from the current word
                    // Selected Cell changes
                    // Selected Word changes
                    if (cell.GetReservedBy()[0] == selectedWord)
                    {
                        selectedWord = cell.GetReservedBy()[1];
                        // Debug.Log("5");
                    }
                    else
                    {
                        selectedWord = cell.GetReservedBy()[0];
                        // Debug.Log("6");
                    }
                }
            }
        }
        else
        {
            // Clicked on a cell apart from the current word
            if (cell.GetReservedBy().Count == 1)
            {
                // Cell belongs to only one word
                // Selected Cell changes
                // Selected Word changes
                selectedWord = cell.GetReservedBy()[0];
                // Debug.Log("7");
            }
            else if (cell.GetReservedBy().Count == 2)
            {
                // Cell Belongs to more than one word
                CrosswordCluePlacementData wordA = cell.GetReservedBy()[0];
                CrosswordCluePlacementData wordB = cell.GetReservedBy()[1];

                bool isFirstCellOfWordA = cell == wordA.GetIncorperatedCells()[0];
                bool isFirstCellOfWordB = cell == wordB.GetIncorperatedCells()[0];

                if (isFirstCellOfWordA && !isFirstCellOfWordB)
                {
                    selectedWord = wordA;
                    // Debug.Log("8");
                }
                else if (!isFirstCellOfWordA && isFirstCellOfWordB)
                {
                    selectedWord = wordB;
                    // Debug.Log("9");
                }
                else
                {
                    // Default to the Horizontal word
                    // Selected Word changes
                    if (wordA.GetAlignment() == Alignment.HORIZONTAL)
                    {
                        selectedWord = wordA;
                        // Debug.Log("10");
                    }
                    else
                    {
                        selectedWord = wordB;
                        // Debug.Log("11");
                    }
                }
            }
        }

        board.ShowCells(selectedWord);
        clueText.text = selectedWord.GetClue().GetClue();
        selectedCell = cell;
        selectedCell.Select();
    }

    private void ToggleInputMode()
    {
        switch (currentInputMode)
        {
            case InputMode.INPUT:
                currentInputMode = InputMode.PENCIL;
                pencilButton.SetState(true);
                break;
            case InputMode.PENCIL:
                currentInputMode = InputMode.INPUT;
                pencilButton.SetState(false);
                break;
        }
    }

    private void SolveSelectedCell()
    {
        if (selectedCell)
        {
            char c = selectedCell.GetCorrectChar();
            selectedCell.SetInputtedChar(c);
            selectedCell.SetCoverColor(Color.white);
            StartCoroutine(selectedCell.ChangeCoverAlpha(0));
            forceUpdate = true;
        }
    }

    private void SolveSelectedWord()
    {
        if (selectedCell)
        {
            List<CrosswordBoardCell> cells = selectedWord.GetIncorperatedCells();
            foreach (CrosswordBoardCell cell in cells)
            {
                cell.SetInputtedChar(cell.GetCorrectChar());
                cell.SetCoverColor(Color.white);
                StartCoroutine(cell.ChangeCoverAlpha(0));
            }
        }
    }

    private void SolveBoard()
    {
        board.CheatBoard();
        forceUpdate = true;
    }

    private void PencilCorrectChars()
    {
        List<CrosswordBoardCell> cells = selectedWord.GetIncorperatedCells();

        List<char> toPencil = new List<char>();
        for (int i = 0; i < cells.Count; i++)
        {
            toPencil.Add(cells[i].GetCorrectChar());
        }

        for (int i = 0; i < cells.Count; i++)
        {
            CrosswordBoardCell cur = cells[i];
            if (!cur.GetInputtedChar().Equals(' ')) continue;
            toPencil.Shuffle();
            for (int p = 0; p < toPencil.Count; p++)
            {
                if (!cur.HasCharPencilled(toPencil[p]))
                    cur.TryPencilChar(toPencil[p]);
            }
        }
    }

    private void CheckBoard()
    {
        board.ActOnEachBoardCell(cell =>
        {
            if (!cell.GetCorrectChar().Equals(CrosswordBoardCell.DefaultChar) && !cell.GetInputtedChar().Equals(' '))
            {
                StartCoroutine(cell.ChangeCoverAlpha(.5f));
                if (cell.GetInputtedChar().Equals(cell.GetCorrectChar()))
                {
                    cell.SetCoverColor(Color.green);
                }
                else
                {
                    cell.SetCoverColor(Color.red);
                }
            }
        });
    }

    protected override IEnumerator GameLoop()
    {
        if (!hasDealtWIthFunctionButtons)
        {
            hasDealtWIthFunctionButtons = true;

            pencilButton = virtualKeyboard.GetAdditionalFuncButton("PENCIL");
            additionalFunctionsDict.Add("PENCIL", ToggleInputMode);

            // Tool Tips
            List<ToolTipDataContainer> toolTips = new List<ToolTipDataContainer>();
            toolTips.Add(new ToolTipDataContainer("Pencil the Correct Letters into the Selected Words Cells", () => CallToolTipFunc(PencilCorrectChars), null));
            toolTips.Add(new ToolTipDataContainer("Check the Board for Errors", () => CallToolTipFunc(CheckBoard), null));
            toolTips.Add(new ToolTipDataContainer("Solve the Selected Cell", () => CallToolTipFunc(SolveSelectedCell), null));
            toolTips.Add(new ToolTipDataContainer("Solve the Selected Word", () => CallToolTipFunc(SolveSelectedWord), null));
            toolTips.Add(new ToolTipDataContainer("Solve the Board", () => CallToolTipFunc(SolveBoard), null));

            spawnToolTipsButton = virtualKeyboard.GetAdditionalFuncButton("SPAWN_TOOLTIPS");
            additionalFunctionsDict.Add("SPAWN_TOOLTIPS", delegate
            {
                if (spawnedToolTips.Count > 0)
                {
                    Destroy(spawnedToolTips[0].transform.parent.parent.gameObject);
                    spawnedToolTips.Clear();
                }
                else
                {
                    spawnedToolTips = SpawnToolTips(toolTips);
                    // Returned order should be the same as spawned
                    spawnedToolTips[0].SetState(selectedCell != null);
                    spawnedToolTips[1].SetState(true);
                    spawnedToolTips[2].SetState(selectedCell != null);
                    spawnedToolTips[3].SetState(selectedCell != null);
                    spawnedToolTips[4].SetState(true);
                }
            });
        }

        string currentFrameString;
        while (true)
        {
            if (forceUpdate)
            {
                if (board.CheckForWin())
                {
                    forceUpdate = false;
                    yield break;
                }
            }

            if (selectedCell)
            {
                if (Input.GetKeyDown(KeyCode.Backspace) || backPressed)
                {
                    AudioManager._Instance.PlayFromSFXDict(onInput);
                    backPressed = false;

                    if (selectedCell)
                    {
                        selectedCell.SetInputtedChar(' ');
                        // Determine if there's another cell previous the current
                        List<CrosswordBoardCell> currentClueCells = selectedWord.GetIncorperatedCells();

                        if (!selectedCell.Equals(currentClueCells[0]))
                        {
                            selectedCell.Deselect();
                            int newCellIndex = currentClueCells.IndexOf(selectedCell);
                            CrosswordBoardCell newCell = currentClueCells[newCellIndex - 1];
                            selectedCell = newCell;
                            selectedCell.Select();
                        }
                    }
                }
                else
                {
                    if (keyPressed)
                    {
                        currentFrameString = key;
                        keyPressed = false;
                    }
                    else
                    {
                        currentFrameString = Input.inputString;
                    }

                    if (currentFrameString.Length > 0)
                    {
                        char c = currentFrameString.ToUpper()[0];
                        switch (currentInputMode)
                        {
                            case InputMode.INPUT:
                                selectedCell.SetInputtedChar(c);

                                selectedCell.SetCoverColor(Color.white);
                                StartCoroutine(selectedCell.ChangeCoverAlpha(0));

                                AudioManager._Instance.PlayFromSFXDict(onInput);

                                if (board.CheckForWin())
                                {
                                    CheckBoard();
                                    yield break;
                                }

                                List<CrosswordBoardCell> wordCells = selectedWord.GetIncorperatedCells();
                                for (int i = 0; i < wordCells.Count; i++)
                                {
                                    if (wordCells[i] == selectedCell)
                                    {
                                        if (i < wordCells.Count - 1)
                                        {
                                            // Select Next Cell
                                            selectedCell.Deselect();
                                            selectedCell = wordCells[i + 1];
                                            selectedCell.Select();
                                            break;
                                        }
                                    }
                                }

                                break;
                            case InputMode.PENCIL:
                                if (!selectedCell.GetInputtedChar().Equals(' '))
                                {
                                    selectedCell.SetInputtedChar(' ');
                                }

                                selectedCell.TryPencilChar(c);

                                selectedCell.SetCoverColor(Color.white);
                                StartCoroutine(selectedCell.ChangeCoverAlpha(0));

                                AudioManager._Instance.PlayFromSFXDict(onInput);
                                break;
                        }
                    }
                }
            }
            else
            {
                keyPressed = false;
            }
            yield return null;
        }
    }
}