using UnityEngine;
using UnityEngine.UI;

public abstract class TwoPlayerBoardCell : BoardCell
{
    protected TwoPlayerCellState currentState;

    protected override void Update()
    {
        base.Update();
        symbolCV.alpha = Mathf.MoveTowards(symbolCV.alpha, targetSymbolAlpha, adjustAlphaSpeed * Time.deltaTime);
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

    protected void UpdateSprite()
    {
        SetImageInfo(symbol, currentState);
    }

    protected void SetImageInfo(Image i, TwoPlayerCellState state)
    {
        TicTacToeBoardCellStateVisualInfo info = TwoPlayerDataDealer._Instance.GetStateVisualInfo(state);
        i.sprite = info.Sprite;
        i.color = info.Color;
    }

    protected override void SetInteractableColor(bool v)
    {
        base.SetInteractableColor(v);
        if (currentState == TwoPlayerCellState.NULL)
        {
            targetSymbolAlpha = v ? 1 : 0;
        }
    }
}
