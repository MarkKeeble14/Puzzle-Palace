using UnityEngine;

public class CellDataDealer : MonoBehaviour
{
    public static CellDataDealer _Instance { get; private set; }

    [SerializeField] protected TicTacToeBoardCellStateVisualInfo nullCellVisualInfo;
    [SerializeField] protected Color winCellColor;
    [SerializeField] protected float uninteractableCellAlpha = .4f;
    [SerializeField] protected float interactableCellAlpha = 1f;

    protected virtual void Awake()
    {
        _Instance = this;
    }

    public Color GetWinCellColor()
    {
        return winCellColor;
    }

    public float GetAlphaOfCell(bool interactable)
    {
        return interactable ? interactableCellAlpha : uninteractableCellAlpha;
    }
}
