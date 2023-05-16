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

    private void Start()
    {
        SetStartColor(true);
    }

    bool symbolAlphaLocked;

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

    public IEnumerator ChangeCoverColor(Color color)
    {
        yield return StartCoroutine(Utils.ChangeColor(cover, color, changeColorSpeed));
    }

    private IEnumerator ChangeAlpha(CanvasGroup cv, float target)
    {
        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(cv, target, adjustAlphaSpeed));
    }

    public IEnumerator ChangeCoverAlpha(float target)
    {
        yield return StartCoroutine(ChangeAlpha(coverCV, target));
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

    public void SetCoverColor(Color color)
    {
        cover.color = color;
    }

    public void SetNull()
    {
        ChangeState(TicTacToeBoardCellState.NULL);
    }

    public void SetToCurrentPlayerSymbol()
    {
        if (!activeManager.AllowMove) return;
        if (currentState != TicTacToeBoardCellState.NULL) return;
        ChangeState(TicTacToeDataDealer._Instance.GetCurrentPlayerSymbol());
        StartCoroutine(activeManager.NotifyOfMove(this));
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
        activeManager.SetImageInfo(symbol, currentState);
    }

    public void SetInteractable(bool v)
    {
        symbol.raycastTarget = v;
        SetStartColor(v);
    }

    private void SetStartColor(bool v)
    {
        Color cellColor = TicTacToeDataDealer._Instance.GetCellColor(v);
        mainImageTargetColor = cellColor;
        if (currentState == TicTacToeBoardCellState.NULL)
        {
            symbolTargetAlpha = v ? 1 : 0;
        }
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
