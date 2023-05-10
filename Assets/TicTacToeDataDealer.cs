using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TicTacToeDataDealer : MonoBehaviour
{
    public static TicTacToeDataDealer _Instance { get; private set; }

    [SerializeField]
    private SerializableDictionary<TicTacToeBoardCellState, TicTacToeBoardCellStateVisualInfo> stateSpriteDict
    = new SerializableDictionary<TicTacToeBoardCellState, TicTacToeBoardCellStateVisualInfo>();

    public GameState CurrentTurn { get; set; }

    private void Awake()
    {
        _Instance = this;
    }

    public TicTacToeBoardCellStateVisualInfo GetStateDisplayInfo(TicTacToeBoardCellState state)
    {
        return stateSpriteDict[state];
    }

    public TicTacToeBoardCellState GetCurrentPlayerSymbol()
    {
        if (CurrentTurn == GameState.P1)
        {
            return TicTacToeBoardCellState.O;
        }
        else if (CurrentTurn == GameState.P2)
        {
            return TicTacToeBoardCellState.X;
        }
        else
        {
            return TicTacToeBoardCellState.NULL;
        }
    }
}
