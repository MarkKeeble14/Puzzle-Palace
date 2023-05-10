using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToeBoardCell : MonoBehaviour
{
    private TicTacToeGameManager.TicTacToeBoardCellState currentState;

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

    public Vector2Int Coordinates;

    public void SetNull()
    {
        ChangeState(TicTacToeGameManager.TicTacToeBoardCellState.NULL);
    }

    public void SetToCurrentPlayerSymbol()
    {
        if (currentState != TicTacToeGameManager.TicTacToeBoardCellState.NULL) return;
        ChangeState(TicTacToeGameManager._Instance.GetCurrentPlayerSymbol());
        TicTacToeGameManager._Instance.NotifyOfMove(this);
    }

    public TicTacToeGameManager.TicTacToeBoardCellState GetState()
    {
        return currentState;
    }

    public void ChangeState(TicTacToeGameManager.TicTacToeBoardCellState state)
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
