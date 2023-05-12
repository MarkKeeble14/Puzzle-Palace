using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordoGameManager : MonoBehaviour
{
    public static WordoGameManager _Instance { get; private set; }
    protected bool gameStarted;

    [SerializeField] private ScreenAnimationController eogScreenAnimationHelper;
    [SerializeField] private ScreenAnimationController beginGameScreenAnimationHelper;
    [SerializeField] private GameObject playButton;

    public void BeginGame()
    {
        gameStarted = true;
    }

    private void Awake()
    {
        _Instance = this;
    }

    private void Start()
    {
        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        playButton.SetActive(true);
        yield return new WaitUntil(() => gameStarted);
        beginGameScreenAnimationHelper.Fade(true);

        while (true)
        {
            // Player Inputs Guess
            // Get Player Guess
            // Check Accuracy of Player Guess
            // Show Result
            // Loop
            yield return null;
        }
    }
}
