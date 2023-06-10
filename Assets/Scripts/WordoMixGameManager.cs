using System.Collections;
using TMPro;
using UnityEngine;

public class WordoMixGameManager : WordoGameManager
{
    [Header("Wordo Mix Settings")]
    [SerializeField] private Vector2 minMaxWordLength = new Vector2(3, 8);

    private string numGuessesHSKey = "NumGuessesForLength";
    [SerializeField] private TextMeshProUGUI hsText;

    private new void Start()
    {
        base.Start();

        SetPossibleWords(s => s.Length >= minMaxWordLength.x && s.Length <= minMaxWordLength.y);
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
        SetPossibleWords(s => s.Length >= minMaxWordLength.x && s.Length <= minMaxWordLength.y && !s.Equals(currentWord));
        yield return null;
    }

}
