using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class WordoGameManager : UsesVirtualKeyboardMiniGameManager
{
    [Header("Game Settings")]
    [SerializeField] private TextAsset wordList;

    [Header("UI Settings")]
    [SerializeField] private float changeAlphaOfInvalidWordTextRate = 1.0f;
    [SerializeField] private float showInvalidWordTextDuration = 3.0f;
    [SerializeField] private float delayBetweenCellSpawns = .0125f;
    [SerializeField] protected string winTextString = "Success!";

    [Header("Spawning")]
    [SerializeField] private Transform parentSpawnedTo;
    [SerializeField] private WordoCell wordoCellPrefab;
    [SerializeField] private WordoRow wordoCellRowPrefab;

    // Data
    private string[] allWordData;
    private List<string> possibleWords;
    private List<WordoRow> spawnedRows;
    private WordoCell[] spawnedCells;
    private WordoCell selectedCell;
    private int currentIndex;
    protected string currentWord;
    protected int numGuesses;
    private float showInvalidWordTextTimer;
    protected int currentWordLength;

    [Header("References")]
    [SerializeField] protected TextMeshProUGUI winText;
    [SerializeField] protected TextMeshProUGUI wordText;
    [SerializeField] protected TextMeshProUGUI numGuessesText;
    [SerializeField] private TextMeshProUGUI invalidWordText;
    [SerializeField] private CanvasGroup invalidWordTextCV;
    [SerializeField] private ManipulateRectTransformOnMouseInput manipGamePlace;
    [SerializeField] private RectTransform scrollViewRect;
    private ScrollRect scrollView;

    [Header("Audio")]
    [SerializeField] private string onInput = "gm_onInput";
    [SerializeField] private string onInvalidGuess = "gm_onInvalidGuess";
    [SerializeField] private string onSuccessfulGuess = "gm_onSuccessfulGuess";

    private AdditionalFuncVirtualKeyboardButton pencilButton;

    private InputMode currentInputMode;

    public enum InputMode
    {
        INPUT,
        PENCIL,
    }

    private new void Awake()
    {
        base.Awake();

        LoadWordList();

        // Get reference and store it
        scrollView = scrollViewRect.GetComponent<ScrollRect>();

        virtualKeyboard.Generate();
        pencilButton = virtualKeyboard.GetAdditionalFuncButton("PENCIL");
        pencilButton.SetColor(WordoDataDealer._Instance.GetInactiveButtonColor());
        additionalFunctionsDict.Add("PENCIL", ToggleInputMode);
    }

    private new void Update()
    {
        base.Update();

        invalidWordTextCV.alpha = Mathf.MoveTowards(invalidWordTextCV.alpha, showInvalidWordTextTimer > 0 ? 1 : 0, Time.deltaTime * changeAlphaOfInvalidWordTextRate);
        if (showInvalidWordTextTimer > 0)
        {
            showInvalidWordTextTimer -= Time.deltaTime;
        }
    }

    private void ToggleInputMode()
    {
        switch (currentInputMode)
        {
            case InputMode.INPUT:
                currentInputMode = InputMode.PENCIL;
                pencilButton.SetColor(WordoDataDealer._Instance.GetActiveButtonColor());
                break;
            case InputMode.PENCIL:
                currentInputMode = InputMode.INPUT;
                pencilButton.SetColor(WordoDataDealer._Instance.GetInactiveButtonColor());
                break;
        }
    }

    protected IEnumerator ClearSpawnedRows()
    {
        for (int i = 0; i < spawnedRows.Count; i++)
        {
            if (i == spawnedRows.Count - 1)
            {
                yield return StartCoroutine(spawnedRows[i].ChangeAlpha(0));
            }
            else
            {
                StartCoroutine(spawnedRows[i].ChangeAlpha(0));
            }
        }

        while (spawnedRows.Count > 0)
        {
            Destroy(spawnedRows[0].gameObject);
            spawnedRows.RemoveAt(0);
            yield return null;
        }
    }

    public void SelectCell(WordoCell cell)
    {
        for (int i = 0; i < spawnedCells.Length; i++)
        {
            if (spawnedCells[i].Equals(cell))
            {
                currentIndex = i;
                break;
            }
        }
        SelectCellBasedOnIndex();
    }

    private void SelectCellBasedOnIndex()
    {
        if (currentIndex < 0) currentIndex = 0;
        if (currentIndex > spawnedCells.Length - 1) currentIndex = spawnedCells.Length - 1;
        if (selectedCell) selectedCell.Deselect();
        selectedCell = spawnedCells[currentIndex];
        selectedCell.Select();
    }

    protected override IEnumerator GameLoop()
    {
        // Choose Word
        currentWord = GetRandomWord();
        spawnedRows = new List<WordoRow>();
        currentWordLength = currentWord.Length;

        // Debug.Log("The Word is: " + currentWord);

        while (true)
        {
            // Spawn number of cells
            WordoRow spawnedRow = Instantiate(wordoCellRowPrefab, parentSpawnedTo);
            StartCoroutine(spawnedRow.ChangeAlpha(1));
            spawnedRows.Add(spawnedRow);

            // Adjust scroll view so that top & bottoms don't cut off game area due to scaling
            if (currentWord.Length > 5)
            {
                float scale = (float)(1.0 - (0.1 * (currentWordLength - 5)));
                spawnedRow.transform.localScale = new Vector3(scale, scale, 1);
            }

            spawnedCells = new WordoCell[currentWord.Length];
            for (int i = 0; i < currentWordLength; i++)
            {
                // Debug.Log(currentWord[i]);
                WordoCell spawned = Instantiate(wordoCellPrefab, spawnedRow.transform);
                spawnedCells[i] = spawned;
                spawned.Set(currentWord, currentWord[i]);
                spawned.AddOnPressedAction(SelectCell);
                yield return new WaitForSeconds(delayBetweenCellSpawns);
            }

            // Set Scroll View to show new row
            scrollView.verticalNormalizedPosition = 0;

            currentIndex = 0;
            SelectCellBasedOnIndex();

            // Player Inputs Guess by changing cells
            // Wait for an allowed word & for player to press enter

            string inputtedWord;
            string currentFrameString;

            ResetVirtualKeyboardFuncs();

            while (true)
            {
                inputtedWord = GetInputtedWord(spawnedCells);

                if (Input.GetKeyDown(KeyCode.Return) || enterPressed)
                {
                    AudioManager._Instance.PlayFromSFXDict(onInput);
                    enterPressed = false;
                    if (IsAcceptedWord(GetInputtedWord(spawnedCells)))
                    {
                        // Debug.Log("Accepted Word; Breaking");
                        break;
                    }
                    else
                    {
                        // Debug.Log(GetInputtedWord(spawnedCells) + " Not Recognized");
                        showInvalidWordTextTimer = showInvalidWordTextDuration;
                        AudioManager._Instance.PlayFromSFXDict(onInvalidGuess);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Backspace) || backPressed)
                {
                    AudioManager._Instance.PlayFromSFXDict(onInput);
                    backPressed = false;
                    if (!selectedCell.GetInputtedChar().Equals(' '))
                    {
                        selectedCell.SetInputtedChar(' ');
                    }
                    else
                    {
                        currentIndex--;
                        SelectCellBasedOnIndex();
                        selectedCell.SetInputtedChar(' ');
                    }
                }
                else
                {
                    // If the player inputs something at this point
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
                        AudioManager._Instance.PlayFromSFXDict(onInput);
                        // If a cell is not selected, select the first empty cell
                        if (!selectedCell)
                        {
                            if (GetIndexOfFirstEmptyCell() == Utils.StandardSentinalValue)
                            {
                                yield return null;
                            }
                            else
                            {
                                currentIndex = GetIndexOfFirstEmptyCell();
                                SelectCellBasedOnIndex();
                            }
                        }

                        switch (currentInputMode)
                        {
                            case InputMode.INPUT:
                                selectedCell.SetInputtedChar(currentFrameString.ToCharArray()[0]);

                                if (currentIndex + 1 < spawnedCells.Length)
                                {
                                    currentIndex++;
                                    SelectCellBasedOnIndex();
                                }
                                break;
                            case InputMode.PENCIL:
                                selectedCell.TryPencilChar(currentFrameString.ToCharArray()[0]);
                                break;
                        }
                    }
                }

                yield return null;
            }

            selectedCell.Deselect();

            numGuesses++;

            // Read Player Guess & Check Accuracy of Player Guess
            // Also Show Result
            foreach (WordoCell cell in spawnedCells)
            {
                yield return StartCoroutine(cell.Check());

                if (cell.IsIncorrectGuess())
                {
                    virtualKeyboard.BlackoutKey(cell.GetInputtedChar().ToString(), true);
                }
            }

            if (inputtedWord.Equals(currentWord))
            {
                // Done 
                // Debug.Log("Successfully Guessed Word");
                AudioManager._Instance.PlayFromSFXDict(onSuccessfulGuess);

                virtualKeyboard.ClearBlackoutKeys();

                yield return StartCoroutine(HandleSuccessfulGuess());

                yield break;
            }
            else
            {
                // Loop
                // Debug.Log("Incorrect Guess");
                yield return null;
            }

        }
    }

    protected abstract IEnumerator HandleSuccessfulGuess();


    private int GetIndexOfFirstEmptyCell()
    {
        for (int i = 0; i < spawnedCells.Length; i++)
        {
            if (spawnedCells[i].IsEmpty())
                return i;
        }
        return -1;
    }

    private string GetInputtedWord(WordoCell[] cells)
    {
        string s = "";
        foreach (WordoCell cell in cells)
        {
            s += cell.GetInputtedChar();
        }
        return s;
    }

    private bool IsAcceptedWord(string s)
    {
        // Inputted word is now entered into s
        // Check if this is an accepted word
        return possibleWords.Contains(s);
    }

    private string GetRandomWord()
    {
        return possibleWords[RandomHelper.RandomIntExclusive(0, possibleWords.Count)];
    }

    protected void LoadWordList()
    {
        // Load word list
        if (wordList != null)
        {
            allWordData = wordList.text.Split('\n');
        }
    }

    protected void SetPossibleWords(Func<string, bool> acceptedWord)
    {
        possibleWords = new List<string>();
        string treatedString;
        foreach (string s in allWordData)
        {
            treatedString = s.ToUpper().Trim();
            if (acceptedWord(treatedString))
            {
                // Debug.Log(treatedString + ", " + treatedString.Length);

                possibleWords.Add(treatedString);
            }
        }
    }
}
