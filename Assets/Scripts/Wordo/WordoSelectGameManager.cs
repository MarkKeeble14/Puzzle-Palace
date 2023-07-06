using System.Collections;
using TMPro;
using UnityEngine;

public class WordoSelectGameManager : WordoGameManager
{
    [Header("Standard Wordo Settings")]
    private string numGuessesHSKey = "NumGuesses";

    [SerializeField] private TextMeshProUGUI hsText;

    public void SetWordoLength(int length)
    {
        SetPossibleWords(s => s.Length == length);
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
        // 
        numGuesses = 0;
        gameHasBeenRestarted = true;

        StartCoroutine(HideKeyboard());

        yield return StartCoroutine(ClearSpawnedRows());
    }

    protected override IEnumerator GameWon()
    {
        SetWinText();
        SetWordText();
        SetDefinitionText();
        SetNumGuessesText();

        // Handle High Score
        if (TrySetHighScore(numGuessesHSKey + currentWordLength, numGuesses, (x, y) => x < y))
        {
            hsText.text = "New High Score For Word of Length: " + currentWordLength + "\n" + numGuesses + " Guess" + Utils.GetPluralization(numGuesses);
        }
        else
        {
            float hs = GetHighScore(numGuessesHSKey + currentWordLength);
            hsText.text = "High Score For Word of Length " + currentWordLength + ": " + "\n" + hs + " Guess" + Utils.GetPluralization(Mathf.RoundToInt(hs));
        }

        yield return null;
    }

    protected override IEnumerator HandleSuccessfulGuess()
    {
        yield return null;
    }
}
