using System.Collections.Generic;
using UnityEngine.UI;

public class TicTacToeBoardCell : TwoPlayerBoardCell
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
