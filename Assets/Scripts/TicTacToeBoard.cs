using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
[RequireComponent(typeof(GridLayoutGroup))]
public class TicTacToeBoard : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private float delayBetweenWinCellAnimations = 0.1f;
    private List<TicTacToeBoardCell> winCellList;
    private TicTacToeBoardCell[,] board;

    [Header("Visual & References")]
    [SerializeField] private TicTacToeBoardCell boardCellPrefab;
    private GridLayoutGroup glGroup;
    [SerializeField] private int cellSize;
    [SerializeField] private float delayBetweenCellSpawns = 0.0f;

    [Header("Audio")]
    [SerializeField] private SimpleAudioClipContainer onCellChange;
    [SerializeField] private SimpleAudioClipContainer onGameWon;
    [SerializeField] private SimpleAudioClipContainer onSpawnCell;
    [SerializeField] private SimpleAudioClipContainer onDoneSpawningCells;

    [Header("On Reveal Win Cell")]
    [SerializeField] private SimpleAudioClipContainer onRevealWinCell;
    [SerializeField] private float pitchIncreasePerCell;

    public bool HasChanged { get; private set; }
    public bool GameWon { get; private set; }
    public bool GameTied { get; private set; }


    public void ActOnEachBoardCell(Action<TicTacToeBoardCell> func)
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLongLength(0); p++)
            {
                func(board[i, p]);
            }
        }
    }

    public IEnumerator ActOnEachBoardCellWithDelay(Action<TicTacToeBoardCell> func, float delay, bool reverseOrder)
    {
        if (reverseOrder)
        {
            for (int i = board.GetLength(0) - 1; i >= 0; i--)
            {
                for (int p = (int)board.GetLongLength(0) - 1; p >= 0; p--)
                {
                    func(board[i, p]);
                    yield return new WaitForSeconds(delay);
                }
            }
        }
        else
        {
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int p = 0; p < board.GetLongLength(0); p++)
                {
                    func(board[i, p]);
                    yield return new WaitForSeconds(delay);
                }
            }
        }
    }

    public void SetUnsetBoardSpritesToSprite(Sprite s)
    {
        ActOnEachBoardCell(cell =>
        {
            if (cell.GetState() == TicTacToeGameManager.TicTacToeBoardCellState.NULL)
            {
                cell.GetImage().sprite = s;
            }
        });
    }

    public void ResetHasChanged()
    {
        HasChanged = false;
    }

    public IEnumerator Generate(int boardSize)
    {
        GameWon = false;
        GameTied = false;
        HasChanged = false;
        winCellList = new List<TicTacToeBoardCell>();

        glGroup = GetComponent<GridLayoutGroup>();
        glGroup.cellSize = new Vector2(cellSize, cellSize);
        glGroup.constraintCount = boardSize;
        glGroup.GetComponent<RectTransform>().sizeDelta = Vector2.one * boardSize * cellSize;

        board = new TicTacToeBoardCell[boardSize, boardSize];
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLongLength(0); p++)
            {
                TicTacToeBoardCell cell = GameObject.Instantiate(boardCellPrefab, transform);
                cell.Coordinates = new Vector2Int(i, p);
                board[i, p] = cell;
                cell.name += "<" + i + ", " + p + ">";
            }
        }

        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLongLength(0); p++)
            {
                onSpawnCell.PlayOneShot();

                board[i, p].SetAnimatorParameterBool("FadeIn", true);

                yield return new WaitForSeconds(delayBetweenCellSpawns);
            }
        }

        onDoneSpawningCells.PlayOneShot();
    }

    public void NotifyOfMove(TicTacToeBoardCell alteredCell)
    {
        onCellChange.PlayOneShot();
        CheckForWin(alteredCell);
        CheckForTie();
        HasChanged = true;
    }

    private bool CheckForHorizontalWin(TicTacToeBoardCell cellToCheck)
    {
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
    }

    private bool CheckForVerticalWin(TicTacToeBoardCell cellToCheck)
    {
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
    }

    private bool CheckForDiagonalWin(TicTacToeBoardCell cellToCheck)
    {
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
    }

    private void CheckForTie()
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLongLength(0); p++)
            {
                // Empty cells exist, therefore not at a stalemate yet
                if (board[i, p].GetState() == TicTacToeGameManager.TicTacToeBoardCellState.NULL)
                {
                    return;
                }
            }
        }
        // Successfully looped through all cells without coming across an empty cell, therefore game is tied
        GameTied = true;
        return;
    }

    private void CheckForWin(TicTacToeBoardCell cellToCheck)
    {
        if (CheckForHorizontalWin(cellToCheck))
        {
            GameWon = true;
            return;
        };
        if (CheckForVerticalWin(cellToCheck))
        {
            GameWon = true;
            return;
        };
        if (CheckForDiagonalWin(cellToCheck))
        {
            GameWon = true;
            return;
        };
    }

    public IEnumerator StartWinCellsAnimation()
    {
        float pitchChange = 0;
        foreach (TicTacToeBoardCell cell in winCellList)
        {
            cell.SetAnimatorParameterBool("PartOfWin", true);

            // Audio
            onRevealWinCell.PlayWithPitchAdjustment(pitchChange);
            pitchChange += pitchIncreasePerCell;

            yield return new WaitForSeconds(delayBetweenWinCellAnimations);
        }
        onGameWon.PlayOneShot();
    }
}
