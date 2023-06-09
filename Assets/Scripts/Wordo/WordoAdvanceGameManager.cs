﻿using System.Collections;
using TMPro;
using UnityEngine;

public class WordoAdvanceGameManager : WordoGameManager
{
    [Header("Wordo Max Settings")]
    [SerializeField] private Vector2Int startFinishWordLength = new Vector2Int(3, 8);

    [SerializeField] private TextMeshProUGUI totalGuessesText;
    [SerializeField] private TextMeshProUGUI endGameButtonText;

    [SerializeField] private TextMeshProUGUI overallGuessesHSText;
    [SerializeField] private TextMeshProUGUI timeTakenHSText;
    private string overallGuessesHSKey = "OverallGuesses";

    private int numTotalGuesses;
    private bool passedAllLevels => currentWordLength > startFinishWordLength.y;

    private float overallTimer;


    private new void Start()
    {
        base.Start();

        SetPossibleWords(s => s.Length == startFinishWordLength.x);
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
            SetPossibleWords(s => s.Length == startFinishWordLength.x);

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
            winText.text = "Level " + (currentWordLength - startFinishWordLength.x) + " " + winTextString;
            SetWordText();
            SetDefinitionText();
            numGuessesText.text = "You Guessed the Word in " + numGuesses + " Guess" + (numGuesses > 1 ? "es" : "");

            endGameButtonText.text = "Next";
        }
        yield return null;
    }

    protected override IEnumerator HandleSuccessfulGuess()
    {
        currentWordLength++;
        SetPossibleWords(s => s.Length == currentWordLength);
        numTotalGuesses += numGuesses;

        gameStarted = false;

        yield return null;
    }
}
