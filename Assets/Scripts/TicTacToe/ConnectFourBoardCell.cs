public class ConnectFourBoardCell : BoardCell
{
    private ConnectFourGameManager activeConnectFourManager;
    private ConnectFourGameManager activeManager
    {
        get
        {
            if (activeConnectFourManager == null)
                activeConnectFourManager = (ConnectFourGameManager)MiniGameManager._Instance;
            return activeConnectFourManager;
        }
    }

    public void MakeMove()
    {
        if (!activeManager.AllowMove) return;
        if (currentState != TwoPlayerCellState.NULL) return;
        StartCoroutine(activeManager.NotifyOfMove(this));
    }
}
