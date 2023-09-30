using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class BoardCell : MonoBehaviour
{
    public Vector2Int Coordinates { get; set; }

    [Header("Settings")]
    [SerializeField] protected float changeColorSpeed = 1.0f;
    [SerializeField] protected float adjustAlphaSpeed = 1.0f;
    [SerializeField] protected float adjustScaleSpeed = 1.0f;
    protected float targetBackgroundAlpha;
    protected float targetSymbolAlpha;

    [Header("References")]
    [SerializeField] protected Button button;
    [SerializeField] protected RectTransform backgroundTransform;
    [SerializeField] protected Image symbol;
    [SerializeField] private Image cover;
    protected CanvasGroup backgroundCV;
    protected CanvasGroup symbolCV;
    private CanvasGroup coverCV;

    private CellDataDealer cachedCellDataDealer;

    // Coroutines
    private Coroutine currentScaleCoroutine;
    private Coroutine currentMainCVAlphaCoroutine;
    private Coroutine currentCoverCVAlphaCoroutine;

    private void Awake()
    {
        backgroundCV = backgroundTransform.GetComponent<CanvasGroup>();
        symbolCV = symbol.GetComponent<CanvasGroup>();
        coverCV = cover.GetComponent<CanvasGroup>();

        // Set scale to 0 initially
        backgroundTransform.localScale = Vector3.zero;
    }

    protected virtual void SetInteractableColor(bool v)
    {
        if (!cachedCellDataDealer) cachedCellDataDealer = FindObjectOfType<CellDataDealer>();
        targetBackgroundAlpha = cachedCellDataDealer.GetAlphaOfCell(v);
    }

    protected virtual void Update()
    {
        backgroundCV.alpha = Mathf.MoveTowards(backgroundCV.alpha, targetBackgroundAlpha, adjustAlphaSpeed * Time.deltaTime);
        if (backgroundCV.alpha < 1)
        {
            backgroundCV.blocksRaycasts = false;
        }
        else
        {
            backgroundCV.blocksRaycasts = true;
        }
    }

    public void SetInteractable(bool v)
    {
        symbol.raycastTarget = v;
        SetInteractableColor(v);
    }

    public IEnumerator ChangeScale(Vector3 targetScale)
    {
        if (currentScaleCoroutine != null) StopCoroutine(currentScaleCoroutine);
        currentScaleCoroutine = StartCoroutine(Utils.ChangeScale(backgroundTransform, targetScale, adjustScaleSpeed));
        yield return currentScaleCoroutine;
    }

    public IEnumerator ChangeScale(float targetScale)
    {
        if (currentScaleCoroutine != null) StopCoroutine(currentScaleCoroutine);
        currentScaleCoroutine = StartCoroutine(ChangeScale(Vector3.one * targetScale));
        yield return currentScaleCoroutine;
    }

    private IEnumerator ChangeAlpha(CanvasGroup cv, float target)
    {
        if (currentMainCVAlphaCoroutine != null) StopCoroutine(currentMainCVAlphaCoroutine);
        currentMainCVAlphaCoroutine = StartCoroutine(Utils.ChangeCanvasGroupAlpha(cv, target, adjustAlphaSpeed));
        yield return currentMainCVAlphaCoroutine;
    }

    public IEnumerator ChangeCoverAlpha(float target)
    {
        if (currentCoverCVAlphaCoroutine != null) StopCoroutine(currentCoverCVAlphaCoroutine);
        currentCoverCVAlphaCoroutine = StartCoroutine(ChangeAlpha(coverCV, target));
        yield return currentCoverCVAlphaCoroutine;
    }

    public IEnumerator PulseCoverAlpha()
    {
        yield return StartCoroutine(ChangeCoverAlpha(1));

        yield return StartCoroutine(ChangeCoverAlpha(0));
    }

    public void SetCoverColor(Color color)
    {
        cover.color = color;
    }
}
