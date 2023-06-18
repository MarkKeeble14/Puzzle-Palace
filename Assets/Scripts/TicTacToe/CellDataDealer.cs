using UnityEngine;

public class CellDataDealer : MonoBehaviour
{
    public static CellDataDealer _Instance { get; private set; }

    [SerializeField] protected TicTacToeBoardCellStateVisualInfo nullCellVisualInfo;
    [SerializeField] protected Color winCellColor;
    [SerializeField] protected Color uninteractableCellColor;
    [SerializeField] protected Color interactableCellColor;

    protected virtual void Awake()
    {
        _Instance = this;
    }

    public Color GetWinCellColor()
    {
        return winCellColor;
    }

    public Color GetCellColor(bool interactable)
    {
        return interactable ? interactableCellColor : uninteractableCellColor;
    }
}
