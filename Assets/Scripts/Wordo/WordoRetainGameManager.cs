using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WordoRetainGameManager : WordoGameManager
{
    [Header("Wordo Retain Settings")]
    [SerializeField] private int wordLength = 5;
    [SerializeField] private int numLevels = 3;

    [SerializeField] private TextMeshProUGUI totalGuessesText;
    [SerializeField] private TextMeshProUGUI endGameButtonText;

    private bool passedAllLevels => currentLevelIndex >= numLevels;
    private int currentLevelIndex;

    [SerializeField] private TextMeshProUGUI overallGuessesHSText;
    [SerializeField] private TextMeshProUGUI timeTakenHSText;
    private string overallGuessesHSKey = "OverallGuesses";

    private int numTotalGuesses;

    private float overallTimer;


    private new void Start()
    {
        base.Start();

        SetPossibleWords(s => s.Length == wordLength);
    }

    protected override IEnumerator Setup()
    {
        yield return StartCoroutine(ShowKeyboard());

        if (gameHasBeenRestarted)
            gameStarted = true;

        yield return null;
    }

    protected override IEnumerator Restart()
    {
        if (passedAllLevels)
        {
            numTotalGuesses = 0;
            SetPossibleWords(s => s.Length == wordLength);

            autoFillCellIndecies.Clear();

            StartCoroutine(HideKeyboard());
        }

        // 
        gameHasBeenRestarted = true;
        numGuesses = 0;

        yield return StartCoroutine(ClearSpawnedRows());
    }

    protected override IEnumerator GameWon()
    {
        overallTimer += timer;
        if (passedAllLevels)
        {
            SetWinText();
            SetWordText();
            SetDefinitionText();
            SetNumGuessesText();

            totalGuessesText.gameObject.SetActive(true);
            totalGuessesText.text = "Overall Guesses: " + numTotalGuesses;

            endGameButtonText.text = "Play Again";

            // Handle High Scores
            if (TrySetHighScore(overallGuessesHSKey, numTotalGuesses, (x, y) => x < y))
            {
                overallGuessesHSText.text = "New High Score!: " + numTotalGuesses + " Total Guess" + Utils.GetPluralization(numTotalGuesses);
            }
            else
            {
                overallGuessesHSText.text = "High Score: " + GetHighScore(overallGuessesHSKey) + " Total Guess" + Utils.GetPluralization(numTotalGuesses);
            }

            SetTimerHighScore(timeTakenHSText, overallTimer);
            overallTimer = 0;
        }
        else
        {
            winText.text = "Level " + (currentLevelIndex) + " " + winTextString;
            SetWordText();
            SetDefinitionText();
            numGuessesText.text = "You Guessed the Word in " + numGuesses + " Guess" + (numGuesses > 1 ? "es" : "");

            endGameButtonText.text = "Next";
        }
        yield return null;
    }

    protected override IEnumerator HandleSuccessfulGuess()
    {
        autoFillCellIndecies.Clear();

        int chosenIndex = RandomHelper.RandomIntExclusive(0, wordLength);
        char c = currentWord[chosenIndex];
        currentLevelIndex++;
        autoFillCellIndecies.Add(chosenIndex);

        SetPossibleWords(s => s.Length == currentWordLength && s[chosenIndex].Equals(c));
        numTotalGuesses += numGuesses;

        gameStarted = false;

        yield return null;
    }
}
