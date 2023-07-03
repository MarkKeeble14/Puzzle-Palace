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

    public IEnumerator ChangeScale(Vector3 targetScale)
    {
        yield return StartCoroutine(Utils.ChangeScale(backgroundTransform, targetScale, adjustScaleSpeed));
    }

    public IEnumerator ChangeScale(float targetScale)
    {
        yield return StartCoroutine(ChangeScale(Vector3.one * targetScale));
    }

    public IEnumerator ChangeAlpha(CanvasGroup cv, float target)
    {
        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(cv, target, adjustAlphaSpeed));
    }

    public void SetInteractable(bool v)
    {
        symbol.raycastTarget = v;
        SetInteractableColor(v);
    }

    public IEnumerator ChangeCoverColor(Color color)
    {
        yield return StartCoroutine(Utils.ChangeColor(cover, color, changeColorSpeed));
    }

    public IEnumerator ChangeCoverAlpha(float target)
    {
        yield return StartCoroutine(ChangeAlpha(coverCV, target));
    }

    public void SetCoverColor(Color color)
    {
        cover.color = color;
    }
}
