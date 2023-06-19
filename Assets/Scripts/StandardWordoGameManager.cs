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

        yield return StartCoroutine(HideKeyboard());

        yield return StartCoroutine(ClearSpawnedRows());
    }

    protected override IEnumerator GameWon()
    {
        SetWinText();
        SetWordText();
        SetDefinitionText();
        SetNumGuessesText();

        // Handle High Score
        if (TrySetHighScore(numGuessesHSKey, numGuesses, (x, y) => x < y))
        {
            hsText.text = "New High Score!: " + numGuesses + " Guess" + Utils.GetPluralization(numGuesses);
        }
        else
        {
            float hs = GetHighScore(numGuessesHSKey);
            hsText.text = "High Score: " + hs + " Guess" + Utils.GetPluralization(Mathf.RoundToInt(hs));
        }

        yield return null;
    }

    protected override IEnumerator HandleSuccessfulGuess()
    {
        yield return null;
    }
}
