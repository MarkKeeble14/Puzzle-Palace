using System.Collections;
using TMPro;
using UnityEngine;

public class StandardWordoGameManager : WordoGameManager
{
    [Header("Standard Wordo Settings")]
    [SerializeField] private int wordLength = 5;

    private string numGuessesHSKey = "NumGuesses";

    [SerializeField] private TextMeshProUGUI hsText;


    private new void Start()
    {
        base.Start();

        SetPossibleWords(s => s.Length == wordLength);
    }

    protected override IEnumerator Setup()
    {
        yield return null;
    }

    protected override IEnumerator Restart()
    {
        // 
        numGuesses = 0;

        yield return StartCoroutine(ClearSpawnedRows());
    }

    protected override IEnumerator GameWon()
    {
        winText.text = winTextString;
        wordText.text = "The Word was " + Utils.CapitalizeFirstLetter(currentWord);
        numGuessesText.text = "You Guessed the Word in " + numGuesses + " Guess" + Utils.GetPluralization(numGuesses);

        // Handle High Score
        if (TrySetHighScore(numGuessesHSKey, numGuesses, (x, y) => x < y))
        {
            hsText.text = "New High Score!: " + numGuesses + " Guess" + Utils.GetPluralization(numGuesses);
        }
        else
        {
            hsText.text = "High Score: " + GetHighScore(numGuessesHSKey);
        }

        yield return null;
    }

    protected override IEnumerator HandleSuccessfulGuess()
    {
        yield return null;
    }
}
