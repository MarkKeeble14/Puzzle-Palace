using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BoardCell : MonoBehaviour
{
    protected TwoPlayerCellState currentState;
    public Vector2Int Coordinates { get; set; }

    [Header("Settings")]
    [SerializeField] protected float changeColorSpeed = 1.0f;
    [SerializeField] protected float adjustAlphaSpeed = 1.0f;
    [SerializeField] protected float adjustScaleSpeed = 1.0f;
    protected Color mainImageTargetColor;
    protected float symbolTargetAlpha;

    [Header("References")]
    [SerializeField] protected RectTransform mainTransform;
    [SerializeField] protected Image mainImage;
    [SerializeField] protected CanvasGroup mainCV;
    [SerializeField] protected Image symbol;
    [SerializeField] protected CanvasGroup symbolCV;
    [SerializeField] protected Button button;
    [SerializeField] private Image cover;
    [SerializeField] private CanvasGroup coverCV;
    private bool symbolAlphaLocked;

    protected void Start()
    {
        SetStartColor(true);
    }

    private void Update()
    {
        mainImage.color = Color.Lerp(mainImage.color, mainImageTargetColor, changeColorSpeed * Time.deltaTime);
        if (!symbolAlphaLocked)
        {
            symbolCV.alpha = Mathf.Lerp(symbolCV.alpha, symbolTargetAlpha, adjustAlphaSpeed * Time.deltaTime);
        };
    }

    public IEnumerator ChangeScale(Vector3 targetScale)
    {
        yield return StartCoroutine(Utils.ChangeScale(mainTransform, targetScale, adjustScaleSpeed));
    }

    public IEnumerator ChangeScale(float targetScale)
    {
        yield return StartCoroutine(ChangeScale(Vector3.one * targetScale));
    }

    public IEnumerator ChangeAlpha(CanvasGroup cv, float target)
    {
        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(cv, target, adjustAlphaSpeed));
    }

    public IEnumerator ChangeTotalAlpha(float target)
    {
        yield return StartCoroutine(ChangeAlpha(mainCV, target));
    }

    public IEnumerator LockSymbolAlpha(float target)
    {
        symbolAlphaLocked = true;
        yield return StartCoroutine(ChangeAlpha(symbolCV, target));
    }

    public void SetInteractable(bool v)
    {
        symbol.raycastTarget = v;
        SetStartColor(v);
    }
    public void HardSetToSymbol(TwoPlayerCellState symbol)
    {
        ChangeState(symbol);
    }

    public TwoPlayerCellState GetState()
    {
        return currentState;
    }

    public void ChangeState(TwoPlayerCellState state)
    {
        currentState = state;
        UpdateSprite();
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

    private void UpdateSprite()
    {
        SetImageInfo(symbol, currentState);
    }

    private void SetImageInfo(Image i, TwoPlayerCellState state)
    {
        TicTacToeBoardCellStateVisualInfo info = TwoPlayerDataDealer._Instance.GetStateVisualInfo(state);
        i.sprite = info.Sprite;
        i.color = info.Color;
    }

    protected void SetStartColor(bool v)
    {
        Color cellColor = TwoPlayerDataDealer._Instance.GetCellColor(v);
        mainImageTargetColor = cellColor;
        if (currentState == TwoPlayerCellState.NULL)
        {
            symbolTargetAlpha = v ? 1 : 0;
        }
    }
}

public class TicTacToeBoardCell : BoardCell
{
    public TicTacToeBoard OwnerOfCell { get; set; }

    private TicTacToeGameManager activeTicTacToeManager;
    private TicTacToeGameManager activeManager
    {
        get
        {
            if (activeTicTacToeManager == null)
                activeTicTacToeManager = (TicTacToeGameManager)MiniGameManager._Instance;
            return activeTicTacToeManager;
        }
    }

    #region Symbol Manipulation

    public void SetToCurrentPlayerSymbol()
    {
        if (!activeManager.AllowMove) return;
        if (currentState != TwoPlayerCellState.NULL) return;
        ChangeState(TwoPlayerDataDealer._Instance.GetCurrentPlayerSymbol());
        StartCoroutine(activeManager.NotifyOfMove(this));
    }

    #endregion

    public void SetNull()
    {
        ChangeState(TwoPlayerCellState.NULL);
    }

    public Image GetSymbolImage()
    {
        return symbol;
    }

    public override string ToString()
    {
        return "<" + Coordinates.x + ", " + Coordinates.y + ">: " + base.ToString();
    }
}
