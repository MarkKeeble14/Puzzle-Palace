using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToeBoardCell : MonoBehaviour
{
    private TicTacToeBoardCellState currentState;
    public Vector2Int Coordinates;
    public TicTacToeBoard OwnerOfCell;


    // References
    [SerializeField] private Image image;
    [SerializeField] private Animator animator;

    public void SetAnimatorParameterBool(string key, bool value)
    {
        animator.SetBool(key, value);
    }

    public Image GetImage()
    {
        return image;
    }


    public void SetNull()
    {
        ChangeState(TicTacToeBoardCellState.NULL);
    }

    public void SetToCurrentPlayerSymbol()
    {
        if (currentState != TicTacToeBoardCellState.NULL) return;
        ChangeState(TicTacToeDataDealer._Instance.GetCurrentPlayerSymbol());
        TicTacToeGameManager._Instance.NotifyOfMove(this);
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
        TicTacToeGameManager._Instance.SetImageInfo(image, currentState);
    }

    public override string ToString()
    {
        return "<" + Coordinates.x + ", " + Coordinates.y + ">: " + base.ToString();
    }
}
