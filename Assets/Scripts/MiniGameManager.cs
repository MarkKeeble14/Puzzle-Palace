using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToolTipDataContainer
{
    public string text;
    public Action onConfirm;
    public Action onCancel;

    public ToolTipDataContainer(string text, Action onConfirm, Action onCancel)
    {
        this.text = text;
        this.onConfirm = onConfirm;
        this.onCancel = onCancel;
    }
}

public abstract class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager _Instance { get; private set; }

    protected bool gameStarted;
    protected float timer;

    [Header("References")]
    [SerializeField] protected ScreenAnimationController beginGameScreen;
    [SerializeField] protected ScreenAnimationController endGameScreen;

    [SerializeField] private string miniGameLabel;
    private string hsLead = "hs_";

    private string timeTakenHSKey = "Duration";

    [SerializeField] private MultiToolTipDisplayController multiToolTipDisplayPrefab;
    [SerializeField] private ToolTipDisplay singleToolTipDisplayPrefab;
    [SerializeField] private Transform toolTipHolder;

    protected void SpawnToolTip(string text, Action onConfirm, Action onCancel)
    {
        ToolTipDisplay spawned = Instantiate(singleToolTipDisplayPrefab, toolTipHolder);
        spawned.SetText(text);
        spawned.AddOnConfirmAction(delegate
        {
            Destroy(spawned.gameObject);
            onConfirm?.Invoke();
        });

        spawned.AddOnCancelAction(delegate
        {
            Destroy(spawned.gameObject);
            onCancel?.Invoke();
        });
    }

    protected List<ToolTipDisplay> SpawnToolTips(List<ToolTipDataContainer> buttonsInfo)
    {
        MultiToolTipDisplayController spawned = Instantiate(multiToolTipDisplayPrefab, toolTipHolder);
        List<ToolTipDisplay> toolTips = new List<ToolTipDisplay>();
        for (int i = 0; i < buttonsInfo.Count; i++)
        {
            toolTips.Add(spawned.Add(buttonsInfo[i]));
        }
        return toolTips;
    }

    private string ConstructHighScoreString(string key)
    {
        return hsLead + miniGameLabel + "_" + key;
    }

    protected bool TrySetHighScore(string key, float v, Func<float, float, bool> comparisonOperator)
    {
        key = ConstructHighScoreString(key);

        if (PlayerPrefs.HasKey(key))
        {
            float prevHS = PlayerPrefs.GetFloat(key);
            if (comparisonOperator(v, prevHS))
            {
                SaveFloatToPlayerPrefs(key, v);
                return true;
            }
        }
        else
        {
            SaveFloatToPlayerPrefs(key, v);
            return true;
        }

        PlayerPrefs.Save();
        return false;
    }

    protected bool HasHighScore(string key)
    {
        return PlayerPrefs.HasKey(ConstructHighScoreString(key));
    }

    protected float GetHighScore(string key)
    {
        return PlayerPrefs.GetFloat(ConstructHighScoreString(key));
    }

    private void SaveFloatToPlayerPrefs(string key, float v)
    {
        PlayerPrefs.SetFloat(key, v);
        PlayerPrefs.Save();
    }

    protected void SetTimerHighScore(TextMeshProUGUI text)
    {
        if (TrySetHighScore(timeTakenHSKey, timer, (x, y) => x < y))
        {
            text.text = "New High Score!: " + Utils.ParseDuration((int)timer);
        }
        else
        {
            text.text = "High Score: " + Utils.ParseDuration((int)GetHighScore(timeTakenHSKey));
        }
    }

    protected void SetTimerHighScore(TextMeshProUGUI text, float overridenTimer)
    {
        if (TrySetHighScore(timeTakenHSKey, overridenTimer, (x, y) => x < y))
        {
            text.text = "New High Score!: " + Utils.ParseDuration((int)overridenTimer);
        }
        else
        {
            text.text = "High Score: " + Utils.ParseDuration((int)GetHighScore(timeTakenHSKey));
        }
    }

    protected void DeleteHighScore(string key)
    {
        PlayerPrefs.DeleteKey(ConstructHighScoreString(key));
    }

    protected void Awake()
    {
        _Instance = this;
    }

    protected void Start()
    {
        StartCoroutine(GameFlow());
    }

    protected void Update()
    {
        if (gameStarted)
            timer += Time.deltaTime;
    }

    public void BeginGame()
    {
        gameStarted = true;
        beginGameScreen.Fade(0.0f);
    }

    public IEnumerator RestartGame()
    {
        endGameScreen.Fade(0.0f);

        yield return StartCoroutine(Restart());

        StartCoroutine(GameFlow());
    }

    protected abstract IEnumerator Restart();

    public IEnumerator CallGameWon()
    {
        endGameScreen.Fade(1.0f);

        yield return StartCoroutine(GameWon());
    }

    protected abstract IEnumerator GameWon();

    private IEnumerator GameFlow()
    {
        yield return StartCoroutine(Setup());

        yield return new WaitUntil(() => gameStarted);

        yield return StartCoroutine(GameLoop());

        yield return StartCoroutine(CallGameWon());

        timer = 0;
        gameStarted = false;
    }

    protected abstract IEnumerator GameLoop();
    protected virtual IEnumerator Setup()
    {
        yield return null;
    }

    [ContextMenu("ClearTimerHighScore")]
    public void ClearTimerHighScore()
    {
        DeleteHighScore(timeTakenHSKey);
    }
}
