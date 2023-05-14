using System;
using System.Collections;
using UnityEngine;

public abstract class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager _Instance { get; private set; }

    protected bool gameStarted;

    [Header("References")]
    [SerializeField] protected ScreenAnimationController endGameScreen;
    [SerializeField] protected ScreenAnimationController beginGameScreen;

    protected void Awake()
    {
        _Instance = this;
    }

    protected void Start()
    {
        StartCoroutine(GameFlow());
    }

    public void BeginGame()
    {
        gameStarted = true;
        beginGameScreen.Fade(true);
    }

    public IEnumerator RestartGame()
    {
        endGameScreen.Fade(false);

        yield return StartCoroutine(Restart());

        StartCoroutine(GameFlow());
    }

    protected abstract IEnumerator Restart();

    public IEnumerator CallGameWon()
    {
        endGameScreen.Fade(true);

        yield return StartCoroutine(GameWon());
    }

    protected abstract IEnumerator GameWon();

    private IEnumerator GameFlow()
    {
        yield return StartCoroutine(Setup());

        yield return new WaitUntil(() => gameStarted);

        yield return StartCoroutine(GameLoop());

        yield return StartCoroutine(CallGameWon());
    }

    protected abstract IEnumerator GameLoop();
    protected virtual IEnumerator Setup()
    {
        yield return null;
    }
}
