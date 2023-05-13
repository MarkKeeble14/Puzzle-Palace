using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordoGameManager : MiniGameManager
{
    [SerializeField] private Transform parentSpawnedTo;
    [SerializeField] private WordoCell wordoCellPrefab;
    [SerializeField] private WordoRow wordoCellRowPrefab;

    private WordoCell selectedCell;
    private WordoCell[] spawnedCells;
    private int currentIndex;

    private int sentinelValue = -1;

    [SerializeField] private TextMeshProUGUI winText;
    [SerializeField] private TextMeshProUGUI wordText;
    [SerializeField] private TextMeshProUGUI numGuessesText;

    [SerializeField] private TextMeshProUGUI invalidWordText;

    private List<WordoRow> spawnedRows;

    [SerializeField] private TextAsset wordList;
    private List<string> possibleWords;

    private string word;
    private int numGuesses;

    [SerializeField] private VirtualKeyboard virtualKeyboard;

    private bool enterPressed;
    private bool backPressed;
    private bool keyPressed;
    private string key;

    [SerializeField] private SimpleAudioClipContainer onInput;
    [SerializeField] private SimpleAudioClipContainer onInvalidWordInput;
    [SerializeField] private SimpleAudioClipContainer onSuccess;

    [SerializeField] private CanvasGroup invalidWordTextCV;

    private float showInvalidWordTextTimer;
    [SerializeField] private float changeAlphaOfInvalidWordTextRate;
    [SerializeField] private float showInvalidWordTextDuration = 3.0f;


    private new void Start()
    {
        LoadWordList();

        base.Start();
    }

    protected override IEnumerator Restart()
    {
        // 
        numGuesses = 0;

        for (int i = 0; i < spawnedRows.Count; i++)
        {
            yield return StartCoroutine(spawnedRows[i].ChangeAlpha(0));
        }

        while (spawnedRows.Count > 0)
        {
            Destroy(spawnedRows[0].gameObject);
            spawnedRows.RemoveAt(0);
            yield return null;
        }
    }

    protected override IEnumerator GameWon()
    {
        wordText.text = "The Word was " + Utils.CapitalizeFirstLetter(word);
        numGuessesText.text = "You Guessed the Word in " + numGuesses + " Guess" + (numGuesses > 1 ? "es" : "");
        yield return null;
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
        word = GetRandomWord();
        spawnedRows = new List<WordoRow>();
        virtualKeyboard.Generate();

        while (true)
        {
            // Spawn number of cells
            WordoRow spawnedRow = Instantiate(wordoCellRowPrefab, parentSpawnedTo);
            StartCoroutine(spawnedRow.ChangeAlpha(1));
            spawnedRows.Add(spawnedRow);

            spawnedCells = new WordoCell[word.Length];
            for (int i = 0; i < word.Length; i++)
            {
                WordoCell spawned = Instantiate(wordoCellPrefab, spawnedRow.transform);
                spawnedCells[i] = spawned;
                spawned.Set(word, word[i]);
                spawned.AddOnPressedAction(SelectCell);
            }

            currentIndex = 0;
            SelectCellBasedOnIndex();

            // Player Inputs Guess by changing cells
            // Wait for an allowed word & for player to press enter

            string inputtedWord;
            string currentFrameString;

            while (true)
            {
                inputtedWord = GetInputtedWord(spawnedCells);

                if (Input.GetKeyDown(KeyCode.Return) || enterPressed)
                {
                    onInput.PlayOneShot();
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
                        onInvalidWordInput.PlayOneShot();
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Backspace) || backPressed)
                {
                    onInput.PlayOneShot();
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
                        onInput.PlayOneShot();
                        // If a cell is not selected, select the first empty cell
                        if (!selectedCell)
                        {
                            if (GetIndexOfFirstEmptyCell() == sentinelValue)
                            {
                                yield return null;
                            }
                            else
                            {
                                currentIndex = GetIndexOfFirstEmptyCell();
                                SelectCellBasedOnIndex();
                            }
                        }

                        selectedCell.SetInputtedChar(currentFrameString.ToCharArray()[0]);

                        if (currentIndex + 1 < spawnedCells.Length)
                        {
                            currentIndex++;
                            SelectCellBasedOnIndex();
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

            if (inputtedWord.Equals(word))
            {
                // Done 
                // Debug.Log("Successfully Guessed Word");
                onSuccess.PlayOneShot();
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
        return possibleWords.Contains(s.ToLower());
    }

    private string GetRandomWord()
    {
        return possibleWords[RandomHelper.RandomIntExclusive(0, possibleWords.Count)].ToUpper();
    }

    private void LoadWordList()
    {
        // Load word list
        if (wordList != null)
        {
            possibleWords = new List<string>();
            possibleWords.AddRange(wordList.text.Split('\n'));
        }
    }

    public void SimulateEnter()
    {
        // Debug.Log("Enter");
        enterPressed = true;
    }

    public void SimulateBack()
    {
        // Debug.Log("Back");
        backPressed = true;
    }

    public void KeyPressed(string s)
    {
        Debug.Log(s);
        key = s;
        keyPressed = true;
    }

    private void Update()
    {

        invalidWordTextCV.alpha = Mathf.MoveTowards(invalidWordTextCV.alpha, showInvalidWordTextTimer > 0 ? 1 : 0, Time.deltaTime * changeAlphaOfInvalidWordTextRate);
        if (showInvalidWordTextTimer > 0)
        {
            showInvalidWordTextTimer -= Time.deltaTime;
        }
    }
}
