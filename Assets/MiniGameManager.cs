using System;
using System.Collections;
using UnityEngine;

public abstract class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager _Instance { get; private set; }

    protected bool gameStarted;
    protected float timer;

    [Header("References")]
    [SerializeField] protected ScreenAnimationController endGameScreen;
    [SerializeField] protected ScreenAnimationController beginGameScreen;

    [SerializeField] private string miniGameLabel;

    protected void SetHighScore(string key, float v)
    {
        PlayerPrefs.SetFloat(miniGameLabel + key, v);
        PlayerPrefs.Save();
    }

    protected void SetHighScore(string key, int v)
    {
        PlayerPrefs.SetInt(miniGameLabel + key, v);
        PlayerPrefs.Save();
    }

    protected void Update()
    {
        if (gameStarted)
            timer += Time.deltaTime;
    }

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
