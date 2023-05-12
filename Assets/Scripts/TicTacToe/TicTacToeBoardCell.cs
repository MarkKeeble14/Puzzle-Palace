using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToeBoardCell : MonoBehaviour
{
    private TicTacToeBoardCellState currentState;
    public Vector2Int Coordinates { get; set; }
    public TicTacToeBoard OwnerOfCell { get; set; }

    [SerializeField] private float changeColorSpeed = 1.0f;
    [SerializeField] private float adjustAlphaSpeed = 1.0f;
    [SerializeField] private float adjustScaleSpeed = 1.0f;
    private Color mainImageTargetColor;
    private float symbolTargetAlpha;

    // References
    [SerializeField] private RectTransform mainTransform;
    [SerializeField] private Image mainImage;
    [SerializeField] private Button button;
    [SerializeField] private CanvasGroup mainCV;
    [SerializeField] private Image symbol;
    [SerializeField] private CanvasGroup symbolCV;
    [SerializeField] private Image cover;
    [SerializeField] private CanvasGroup coverCV;

    private void Awake()
    {
        SetInteractable(true);
    }

    public IEnumerator ChangeScale(Vector3 targetScale)
    {
        while (mainTransform.localScale != targetScale)
        {
            mainTransform.localScale = Vector3.MoveTowards(mainTransform.localScale, targetScale, Time.deltaTime * adjustScaleSpeed);
            yield return null;
        }
    }

    public IEnumerator ChangeScale(float targetScale)
    {
        yield return StartCoroutine(ChangeScale(new Vector3(targetScale, targetScale, targetScale)));
    }

    public void SetCoverColor(Color color)
    {
        cover.color = color;
    }

    public IEnumerator ChangeCoverColor(Color color)
    {
        while (cover.color != color)
        {
            cover.color = Color.Lerp(cover.color, color, Time.deltaTime * changeColorSpeed);
            yield return null;
        }
    }

    private IEnumerator ChangeAlpha(CanvasGroup cv, float target)
    {
        while (cv.alpha != target)
        {
            cv.alpha = Mathf.MoveTowards(cv.alpha, target, Time.deltaTime * adjustAlphaSpeed);
            yield return null;
        }
    }

    public IEnumerator ChangeCoverAlpha(float target)
    {
        yield return StartCoroutine(ChangeAlpha(coverCV, target));
    }

    bool symbolAlphaLocked;
    public IEnumerator LockSymbolAlpha(float target)
    {
        symbolAlphaLocked = true;
        yield return StartCoroutine(ChangeAlpha(symbolCV, target));
    }

    public IEnumerator ChangeTotalAlpha(float target)
    {
        yield return StartCoroutine(ChangeAlpha(mainCV, target));
    }

    public Image GetSymbolImage()
    {
        return symbol;
    }

    public void SetNull()
    {
        ChangeState(TicTacToeBoardCellState.NULL);
    }

    public void SetToCurrentPlayerSymbol()
    {
        if (!TicTacToeGameManager._Instance.AllowMove) return;
        if (currentState != TicTacToeBoardCellState.NULL) return;
        ChangeState(TicTacToeDataDealer._Instance.GetCurrentPlayerSymbol());
        StartCoroutine(TicTacToeGameManager._Instance.NotifyOfMove(this));
    }

    public void HardSetToSymbol(TicTacToeBoardCellState symbol)
    {
        ChangeState(symbol);
    }

    public TicTacToeBoardCellState GetState()
    {
        return currentState;
    }

    public void ChangeState(TicTacToeBoardCellState state)
    {
        currentState = state;
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        TicTacToeGameManager._Instance.SetImageInfo(symbol, currentState);
    }

    public override string ToString()
    {
        return "<" + Coordinates.x + ", " + Coordinates.y + ">: " + base.ToString();
    }

    public void SetInteractable(bool v)
    {
        symbol.raycastTarget = v;
        Color cellColor = TicTacToeDataDealer._Instance.GetCellColor(v);
        mainImageTargetColor = cellColor;

        if (currentState == TicTacToeBoardCellState.NULL)
        {
            symbolTargetAlpha = v ? 1 : 0;
        }
    }

    private void Update()
    {
        mainImage.color = Color.Lerp(mainImage.color, mainImageTargetColor, changeColorSpeed * Time.deltaTime);
        if (symbolAlphaLocked) return;
        symbolCV.alpha = Mathf.Lerp(symbolCV.alpha, symbolTargetAlpha, adjustAlphaSpeed * Time.deltaTime);
    }
}
