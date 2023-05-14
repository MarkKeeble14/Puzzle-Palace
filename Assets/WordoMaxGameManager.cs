using System.Collections;
using TMPro;
using UnityEngine;

public class WordoMaxGameManager : WordoGameManager
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

    private new void Start()
    {
        base.Start();

        SetPossibleWords(s => s.Length == startFinishWordLength.x);
    }

    protected override IEnumerator Setup()
    {
        yield return null;
    }

    protected override IEnumerator Restart()
    {
        if (passedAllLevels)
        {
            numTotalGuesses = 0;
            SetPossibleWords(s => s.Length == startFinishWordLength.x);
        }

        // 
        numGuesses = 0;
        gameStarted = true;

        yield return StartCoroutine(ClearSpawnedRows());
    }

    protected override IEnumerator GameWon()
    {
        if (passedAllLevels)
        {
            winText.text = winTextString;
            wordText.text = "The Word was " + Utils.CapitalizeFirstLetter(currentWord);
            numGuessesText.text = "You Guessed the Word in " + numGuesses + " Guess" + (numGuesses > 1 ? "es" : "");

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

            SetTimerHighScore(timeTakenHSText);
        }
        else
        {
            winText.text = "Level " + (currentWordLength - startFinishWordLength.x) + " " + winTextString;
            wordText.text = "The Word was " + Utils.CapitalizeFirstLetter(currentWord);
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
