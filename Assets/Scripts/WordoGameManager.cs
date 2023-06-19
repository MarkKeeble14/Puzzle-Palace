using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class WordoGameManager : UsesVirtualKeyboardMiniGameManager
{
    [Header("Testing")]
    [SerializeField] private bool overrideRandomWord;
    [SerializeField] private string overridenRandomWord;

    [Header("UI Settings")]
    [SerializeField] private bool treatDefinitions = true;
    [SerializeField] private bool limitNumDefinitions = false;
    [SerializeField] private int maxNumDefinitions = 1;
    [SerializeField] private float changeAlphaOfInvalidWordTextRate = 1.0f;
    [SerializeField] private float showInvalidWordTextDuration = 3.0f;
    [SerializeField] private float delayBetweenCellSpawns = .0125f;
    [SerializeField] protected string winTextString = "Success!";

    [Header("Spawning")]
    [SerializeField] private Transform parentSpawnedTo;
    [SerializeField] private WordoCell wordoCellPrefab;
    [SerializeField] private WordoRow wordoCellRowPrefab;

    // Data
    [SerializeField] private TextAsset allAllowedAnswersFile;
    [SerializeField] private TextAsset allGuessableAnswersFile;
    private Dictionary<string, string> wordMap = new Dictionary<string, string>();
    private List<string> allAllowedAnswers;
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
    [SerializeField] protected TextMeshProUGUI definitionText;
    [SerializeField] protected TextMeshProUGUI numGuessesText;
    [SerializeField] private TextMeshProUGUI invalidWordText;
    [SerializeField] private CanvasGroup invalidWordTextCV;
    [SerializeField] private ManipulateRectTransformOnMouseInput manipGamePlace;
    [SerializeField] private RectTransform scrollViewRect;
    private ScrollRect scrollView;
    [SerializeField] private ScrollRect endGameScrollView;

    [Header("Audio")]
    [SerializeField] private string onInput = "gm_onInput";
    [SerializeField] private string onInvalidGuess = "gm_onInvalidGuess";
    [SerializeField] private string onSuccessfulGuess = "gm_onSuccessfulGuess";

    private AdditionalFuncVirtualKeyboardButton pencilButton;

    private InputMode currentInputMode;
    private bool hasDealtWIthPencilButton;
    protected bool gameHasBeenRestarted;

    public enum InputMode
    {
        INPUT,
        PENCIL,
    }

    private new void Awake()
    {
        base.Awake();

        LoadWordMap();

        // Get reference and store it
        scrollView = scrollViewRect.GetComponent<ScrollRect>();
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
                pencilButton.SetState(true);
                break;
            case InputMode.PENCIL:
                currentInputMode = InputMode.INPUT;
                pencilButton.SetState(false);
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
        if (!hasDealtWIthPencilButton)
        {
            hasDealtWIthPencilButton = true;
            pencilButton = virtualKeyboard.GetAdditionalFuncButton("PENCIL");
            additionalFunctionsDict.Add("PENCIL", ToggleInputMode);
        }

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
                    enterPressed = false;
                    if (IsAcceptedWord(GetInputtedWord(spawnedCells)))
                    {
                        AudioManager._Instance.PlayFromSFXDict(onInput);
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

            Dictionary<char, int> inputtedCharAppearancesMap = new Dictionary<char, int>();
            Dictionary<char, int> answerCharAppearancesMap = new Dictionary<char, int>();
            Dictionary<char, int> numCorrectCharsConfirmed = new Dictionary<char, int>();

            for (int i = 0; i < spawnedCells.Length; i++)
            {
                WordoCell cell = spawnedCells[i];
                char inputtedChar = cell.GetInputtedChar();
                char correctChar = cell.GetCorrectChar();

                if (!numCorrectCharsConfirmed.ContainsKey(inputtedChar))
                {
                    numCorrectCharsConfirmed.Add(inputtedChar, 0);
                }

                if (inputtedCharAppearancesMap.ContainsKey(inputtedChar))
                {
                    inputtedCharAppearancesMap[inputtedChar] = inputtedCharAppearancesMap[inputtedChar] + 1;
                }
                else
                {
                    inputtedCharAppearancesMap.Add(inputtedChar, 1);
                }

                if (answerCharAppearancesMap.ContainsKey(correctChar))
                {
                    answerCharAppearancesMap[correctChar] = answerCharAppearancesMap[correctChar] + 1;
                }
                else
                {
                    answerCharAppearancesMap.Add(correctChar, 1);
                }

                if (inputtedChar.Equals(correctChar))
                {
                    numCorrectCharsConfirmed[inputtedChar] += 1;
                }
            }

            foreach (WordoCell cell in spawnedCells)
            {
                WordoCellResult result;
                if (cell.HasCorrectChar())
                {
                    // Debug.Log("1");
                    result = WordoCellResult.CORRECT;
                    numCorrectCharsConfirmed[cell.GetInputtedChar()] += 1;
                    // Debug.Log("Increased NumCorrectCharsConfirmed of " + cell.GetInputtedChar() + ", New Value: " + numCorrectCharsConfirmed[cell.GetInputtedChar()]);
                }
                else if (!answerCharAppearancesMap.ContainsKey(cell.GetInputtedChar()))
                {
                    result = WordoCellResult.INCORRECT;
                    virtualKeyboard.BlackoutKey(cell.GetInputtedChar().ToString(), true);
                    // Debug.Log("2");
                }
                else if (inputtedCharAppearancesMap.ContainsKey(cell.GetInputtedChar()) &&
                    answerCharAppearancesMap.ContainsKey(cell.GetInputtedChar()))
                {
                    // Partially Correct Case
                    if (inputtedCharAppearancesMap[cell.GetInputtedChar()] == answerCharAppearancesMap[cell.GetInputtedChar()])
                    {
                        // Debug.Log("3");
                        result = WordoCellResult.PARTIAL_CORRECT;
                        numCorrectCharsConfirmed[cell.GetInputtedChar()] += 1;
                        // Debug.Log("Increased NumCorrectCharsConfirmed of " + cell.GetInputtedChar() + ", New Value: " + numCorrectCharsConfirmed[cell.GetInputtedChar()]);
                    }
                    else
                    {
                        // Debug.Log(numCorrectCharsConfirmed[cell.GetInputtedChar()] + ", " + answerCharAppearancesMap[cell.GetInputtedChar()]);
                        // Find the number of times it's been placed correctly
                        // We will say that <the number of times it appears - the number of times it's placed correctly> is the number of those char's we must partially accept
                        if (numCorrectCharsConfirmed[cell.GetInputtedChar()] < answerCharAppearancesMap[cell.GetInputtedChar()])
                        {
                            // Debug.Log("4");
                            numCorrectCharsConfirmed[cell.GetInputtedChar()] += 1;
                            result = WordoCellResult.PARTIAL_CORRECT;
                            // Debug.Log("Increased NumCorrectCharsConfirmed of " + cell.GetInputtedChar() + ", New Value: " + numCorrectCharsConfirmed[cell.GetInputtedChar()]);
                        }
                        else
                        {
                            result = WordoCellResult.INCORRECT;
                            // Debug.Log("5");
                        }
                    }
                }
                else
                {
                    result = WordoCellResult.INCORRECT;
                    // Debug.Log("6");
                }

                yield return StartCoroutine(cell.SetResult(result));
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
        if (overrideRandomWord)
        {
            return overridenRandomWord;
        }

        int i = RandomHelper.RandomIntExclusive(0, possibleWords.Count);
        string word = possibleWords[i];
        if (allAllowedAnswers.Contains(word) && wordMap.ContainsKey(word))
        {
            Debug.Log("Set Word: " + word);
            return word;
        }
        else
        {
            if (allAllowedAnswers.Contains(word))
            {
                // Debug.Log("Unacceptable Word: " + word + ", Was in Allowed Answers but not Word Map");
                // wordMap.Remove(word);
            }
            else if (wordMap.ContainsKey(word))
            {
                // Debug.Log("Unacceptable Word: " + word + ", Was in Word Map but not Acceptable Answers");
                allAllowedAnswers.Remove(word);
            }
            return GetRandomWord();
        }
    }

    protected void LoadWordMap()
    {
        if (allAllowedAnswersFile == null)
        {
            throw new Exception("No Answers File Specified");
        }

        allAllowedAnswers = new List<string>();
        allAllowedAnswers.AddRange(allAllowedAnswersFile.text.ToUpper().Split('\n'));

        LoadJson();
    }

    protected void SetPossibleWords(Func<string, bool> acceptedWord)
    {
        possibleWords = new List<string>();
        string treatedString;
        foreach (KeyValuePair<string, string> word in wordMap)
        {
            treatedString = word.Key.ToUpper().Trim();
            if (acceptedWord(treatedString))
            {
                possibleWords.Add(treatedString);
            }
        }
    }

    private void PrintWordMap()
    {
        foreach (KeyValuePair<string, string> kvp in wordMap)
        {
            Debug.Log("Word: " + kvp.Key + ", Definition: " + kvp.Value);
        }
    }

    public void LoadJson()
    {
        string json = allGuessableAnswersFile.text;

        string curWord = "";
        using (var reader = new JsonTextReader(new StringReader(json)))
        {
            while (reader.Read())
            {
                if (reader.Value == null)
                    continue;
                if (!curWord.Equals(""))
                {
                    wordMap.Add(curWord.ToUpper(), reader.Value.ToString());
                    curWord = "";
                }
                else
                {
                    curWord = reader.Value.ToString();
                }
            }
        }
    }

    protected void SetWordText()
    {
        wordText.text = "The Word was " + Utils.CapitalizeFirstLetter(currentWord);
    }


    protected void SetDefinitionText()
    {
        definitionText.text = "Definition:\n" + GetDefinition(currentWord);

        // Set Scroll View to show top
        endGameScrollView.verticalNormalizedPosition = 1;
    }

    protected void SetNumGuessesText()
    {
        numGuessesText.text = "You Guessed the Word in " + numGuesses + " Guess" + (numGuesses > 1 ? "es" : "");
    }

    protected void SetWinText()
    {
        winText.text = winTextString;
    }
    private string GetDefinition(string word)
    {
        if (!treatDefinitions) return wordMap[word];
        string def = wordMap[word];

        if (def.Contains("1"))
        {
            string treatedDef = "";

            if (limitNumDefinitions)
            {
                for (int i = 0; i < def.Length; i++)
                {
                    if (char.IsDigit(def[i]) && def[i + 1].Equals('.') && (i > 0 ? def[i - 1].Equals(' ') : true))
                    {
                        if (i > 0)
                            treatedDef += "\n";
                        treatedDef += "\n";
                    }

                    if (def[i].Equals(char.Parse((1 + maxNumDefinitions).ToString())) && def[i + 1].Equals('.') && (i > 0 ? def[i - 1].Equals(' ') : true))
                    {
                        return treatedDef;
                    }

                    treatedDef += def[i];
                }
                return treatedDef;
            }
            else
            {
                int startAt;
                if (def.Contains('2'))
                {
                    startAt = 0;
                }
                else
                {
                    startAt = 3;
                }

                for (int i = startAt; i < def.Length; i++)
                {
                    if (char.IsDigit(def[i]) && def[i + 1].Equals('.') && (i > 0 ? def[i - 1].Equals(' ') : true))
                    {
                        if (i > 0)
                            treatedDef += "\n";
                        treatedDef += "\n";
                    }
                    treatedDef += def[i];
                }
                return treatedDef;
            }
        }
        else
        {
            return "\n" + def;
        }
    }
}
