using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class UltimateTicTacToeBoard : MonoBehaviour
{
    [Header("Data")]
    private TicTacToeBoard[,] boards;

    [Header("Visual & References")]
    [SerializeField] private TicTacToeBoard boardPrefab;
    [SerializeField] private int boardSize;
    [SerializeField] private float delayBetweenBoardSpawns = 0.0f;
    private GridLayoutGroup glGroup;
    [SerializeField] private float glGroupSpacing = 25.0f;

    public bool HasChanged { get; private set; }
    public bool GameWon { get; private set; }
    public bool GameTied { get; private set; }

    public void ActOnEachBoardCell(Action<TicTacToeBoardCell> func)
    {
        for (int i = 0; i < boards.GetLength(0); i++)
        {
            for (int p = 0; p < boards.GetLongLength(0); p++)
            {
                boards[i, p].ActOnEachBoardCell(func);
            }
        }
    }

    public IEnumerator ActOnEachBoardCellWithDelay(Action<TicTacToeBoardCell> func, float delay, bool reverseOrder)
    {
        if (reverseOrder)
        {
            for (int i = boards.GetLength(0) - 1; i >= 0; i--)
            {
                for (int p = (int)boards.GetLongLength(0) - 1; p >= 0; p--)
                {
                    boards[i, p].ActOnEachBoardCell(func);
                    yield return new WaitForSeconds(delay);
                }
            }
        }
        else
        {
            for (int i = 0; i < boards.GetLength(0); i++)
            {
                for (int p = 0; p < boards.GetLongLength(0); p++)
                {
                    boards[i, p].ActOnEachBoardCell(func);
                    yield return new WaitForSeconds(delay);
                }
            }
        }
    }

    public void SetUnsetBoardSpritesToSprite(Sprite s)
    {
        ActOnEachBoardCell(cell =>
        {
            if (cell.GetState() == TicTacToeBoardCellState.NULL)
            {
                cell.GetImage().sprite = s;
            }
        });
    }

    public void ResetHasChanged()
    {
        HasChanged = false;
    }

    public IEnumerator Generate(int numBoards, int numCells)
    {
        GameWon = false;
        GameTied = false;
        HasChanged = false;

        glGroup = GetComponent<GridLayoutGroup>();
        glGroup.cellSize = new Vector2(boardSize, boardSize);
        glGroup.constraintCount = numBoards;
        glGroup.spacing = glGroupSpacing * Vector2.one;
        glGroup.GetComponent<RectTransform>().sizeDelta = Vector2.one * numBoards * boardSize;

        boards = new TicTacToeBoard[numBoards, numBoards];
        for (int i = 0; i < boards.GetLength(0); i++)
        {
            for (int p = 0; p < boards.GetLongLength(0); p++)
            {
                TicTacToeBoard board = Instantiate(boardPrefab, transform);
                board.Coordinates = new Vector2Int(i, p);
                StartCoroutine(board.Generate(numCells));
                boards[i, p] = board;
                board.name += "<" + i + ", " + p + ">";
            }
        }
        yield return null;
    }


    public void NotifyOfMove(TicTacToeBoardCell alteredCell)
    {
        CheckForWin(alteredCell);
        CheckForTie();
        HasChanged = true;
    }

    private bool CheckForHorizontalWin(TicTacToeBoardCell cellToCheck)
    {
        /*
        // Check for Row Win
        int row = cellToCheck.Coordinates.x;
        for (int i = 0; i < board.GetLength(0); i++)
        {
            TicTacToeBoardCell currentlyChecking = board[row, i];
            if (currentlyChecking.GetState() != cellToCheck.GetState())
            {
                return false;
            }
        }

        // if we are here, that means a horizontal win has occured
        for (int i = 0; i < board.GetLength(0); i++)
        {
            winCellList.Add(board[row, i]);
        }

        return true;
        */
        return false;
    }

    private bool CheckForVerticalWin(TicTacToeBoardCell cellToCheck)
    {
        /*
        // Check for Column Win
        int column = cellToCheck.Coordinates.y;
        for (int i = 0; i < board.GetLength(0); i++)
        {
            TicTacToeBoardCell currentlyChecking = board[i, column];
            if (currentlyChecking.GetState() != cellToCheck.GetState())
            {
                return false;
            }
        }

        // if we are here, that means a vertical win has occured
        for (int i = 0; i < board.GetLength(0); i++)
        {
            winCellList.Add(board[i, column]);
        }
        return true;
        */
        return false;
    }

    private bool CheckForDiagonalWin(TicTacToeBoardCell cellToCheck)
    {
        /*
        bool won = true;
        //
        for (int i = 0; i < board.GetLength(0); i++)
        {
            TicTacToeBoardCell currentlyChecking = board[i, i];
            if (currentlyChecking.GetState() != cellToCheck.GetState())
            {
                won = false;
                break;
            }
        }
        if (won)
        {
            // if we are here, that means a diagonal win in the top left -> bottom right orientationhas occured
            for (int i = 0; i < board.GetLength(0); i++)
            {
                winCellList.Add(board[i, i]);
            }
            return true;
        }

        won = true;
        // 
        for (int i = 0; i < board.GetLength(0); i++)
        {
            TicTacToeBoardCell currentlyChecking = board[i, board.GetLength(0) - 1 - i];
            if (currentlyChecking.GetState() != cellToCheck.GetState())
            {
                won = false;
                break;
            }
        }
        if (won)
        {
            // if we are here, that means a diagonal win in the top left -> bottom right orientationhas occured
            for (int i = 0; i < board.GetLength(0); i++)
            {
                winCellList.Add(board[i, board.GetLength(0) - 1 - i]);
            }
            return true;
        }
        return false;
        */
        return false;
    }

    private void CheckForTie()
    {
        for (int i = 0; i < boards.GetLength(0); i++)
        {
            for (int p = 0; p < boards.GetLongLength(0); p++)
            {
                /*
                // Empty cells exist, therefore not at a stalemate yet
                if (boards[i, p].GetState() == TicTacToeGameManager.TicTacToeBoardCellState.NULL)
                {
                    return;
                }
                */
            }
        }
        // Successfully looped through all cells without coming across an empty cell, therefore game is tied
        GameTied = true;
        return;
    }

    private void CheckForWin(TicTacToeBoardCell cellToCheck)
    {
        if (cellToCheck.OwnerOfCell.GameWon)
        {
            Debug.Log(cellToCheck.OwnerOfCell.ToString() + " Resolved");
        }
    }
}
