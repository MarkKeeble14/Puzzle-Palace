using System;
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

    [Header("References")]
    [SerializeField] private TextAsset clueFile;
    [SerializeField] protected TextMeshProUGUI timeTakenText;
    [SerializeField] protected TextMeshProUGUI hsTimeTakenText;
    [SerializeField] private TextMeshProUGUI clueText;
    [SerializeField] private ManipulateRectTransformOnMouseInput manipGamePlace;
    [SerializeField] private RectTransform scrollViewRect;
    [SerializeField] private ScreenAnimationController hideEndScreenController;
    List<ToolTipDisplay> spawnedToolTips;
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

        yield return StartCoroutine(ShowKeyboard());

        if (gameHasBeenRestarted)
            gameStarted = true;
    }

    protected override IEnumerator Restart()
    {
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

        LoadClueInfo();
    }

    private void LoadClueInfo()
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
        string st = clueFile.text;
        string[] data = st.Split(new char[] { '\t', '\n' });

        // Populate Lists
        int start = MathHelper.RoundToNearestGivenInt(RandomHelper.RandomIntExclusive(0, data.Length - (numCluesToParse * parseFileIncrement)), 4);
        int cap = start + (numCluesToParse * parseFileIncrement);
        // Debug.Log("Total: " + data.Length + ", NumEntries: " + numEntries + ", inc: " + inc + ", Start: " + start);
        for (int i = start; i < cap; i += parseFileIncrement)
        {
            string answer = data[i + 2];
            // Debug.Log(answer);
            if (answer.Length < minMaxWordSize.x || answer.Length > minMaxWordSize.y)
            {
                continue;
            }
            // Debug.Log(new CrosswordClue(data[i + 3], data[i + 2]));
            possibleAnswers.Add(answer);
            clueDict[answer.Length].Add(new CrosswordClue(data[i + 3], answer));
            // Debug.Log("Added Answer: " + answer);
            // in case the increment causes indexer to rise above the cap before filling in all the entries
            if (i + parseFileIncrement > cap && possibleAnswers.Count < numCluesToParse)
            {
                i = 4;
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
            }
            else if (cell.GetReservedBy().Count == 2)
            {
                // Clicked on a cell that belongs to the current word as well as another word
                // Show the word that is different from the current word
                // Selected Cell changes
                // Selected Word changes
                if (cell.GetReservedBy()[0] == selectedWord)
                {
                    selectedWord = cell.GetReservedBy()[1];
                }
                else
                {
                    selectedWord = cell.GetReservedBy()[0];
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
                selectedWord = cell.GetReservedBy()[0];
                // Selected Word changes
            }
            else if (cell.GetReservedBy().Count == 2)
            {
                // Cell Belongs to more than one word
                // Default to the Horizontal word
                // Selected Word changes
                if (cell.GetReservedBy()[0].GetAlignment() == Alignment.HORIZONTAL)
                {
                    selectedWord = cell.GetReservedBy()[0];
                }
                else
                {
                    selectedWord = cell.GetReservedBy()[1];
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
            spawnedToolTips = new List<ToolTipDisplay>();
            hasDealtWIthFunctionButtons = true;

            pencilButton = virtualKeyboard.GetAdditionalFuncButton("PENCIL");
            additionalFunctionsDict.Add("PENCIL", ToggleInputMode);

            // Tool Tips
            List<ToolTipDataContainer> toolTips = new List<ToolTipDataContainer>();
            toolTips.Add(new ToolTipDataContainer("Pencil the Correct Letters into the Selected Words Cells", PencilCorrectChars, null));
            toolTips.Add(new ToolTipDataContainer("Check the Board for Errors", CheckBoard, null));
            toolTips.Add(new ToolTipDataContainer("Solve the Selected Cell", SolveSelectedCell, null));
            toolTips.Add(new ToolTipDataContainer("Solve the Selected Word", SolveSelectedWord, null));
            toolTips.Add(new ToolTipDataContainer("Solve the Board", SolveBoard, null));

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
            /*
            solveCellButton.SetState(selectedCell != null);
            solveCellButton.SetInteractable(selectedCell != null);

            solveWordButton.SetState(selectedCell != null);
            solveWordButton.SetInteractable(selectedCell != null);

            pencilCorrectCharsButton.SetState(selectedCell != null);
            pencilCorrectCharsButton.SetInteractable(selectedCell != null);
            */

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