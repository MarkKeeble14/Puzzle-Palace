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
    // [SerializeField] private float delayBetweenBoardSpawns = 0.0f;
    private GridLayoutGroup glGroup;
    [SerializeField] private float glGroupSpacing = 25.0f;
    [SerializeField] private float delayBetweenWinBoardAnimations = 0.0f;
    [SerializeField] private float pitchIncreasePerCell;

    public bool HasChanged { get; private set; }
    public bool GameWon { get; private set; }
    public bool GameTied { get; private set; }
    public WinnerOptions WinnerState { get; private set; }

    [Header("Audio")]
    [SerializeField] private string onGameWon = "u_tttBoard_onGameWon";
    [SerializeField] private string onRevealWinningBoard = "u_tttBoard_onRevealWinningBoard";

    [Header("On Reveal Win Cell")]
    private List<TicTacToeBoard> winBoardList;
    [SerializeField] private float delayAfterBoardWin = .5f;

    public void ActOnEachBoard(Action<TicTacToeBoard> func)
    {
        for (int i = 0; i < boards.GetLength(0); i++)
        {
            for (int p = 0; p < boards.GetLongLength(0); p++)
            {
                func(boards[i, p]);
            }
        }
    }

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
            if (cell.GetState() == TwoPlayerCellState.NULL)
            {
                cell.GetSymbolImage().sprite = s;
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

        winBoardList = new List<TicTacToeBoard>();

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
                StartCoroutine(board.Generate(numCells, true));
                boards[i, p] = board;
                board.name += "<" + i + ", " + p + ">";
                // yield return new WaitForSeconds(delayBetweenBoardSpawns);
            }
        }
        yield return null;
    }

    private bool CheckForHorizontalWin(TicTacToeBoard boardToCheck)
    {
        if (boardToCheck.WinnerState == WinnerOptions.NEITHER) return false;
        // Check for Row Win
        int row = boardToCheck.Coordinates.x;
        for (int i = 0; i < boards.GetLength(0); i++)
        {
            TicTacToeBoard currentlyChecking = boards[row, i];
            if (currentlyChecking.WinnerState != boardToCheck.WinnerState)
            {
                return false;
            }
        }

        // if we are here, that means a horizontal win has occured
        for (int i = 0; i < boards.GetLength(0); i++)
        {
            winBoardList.Add(boards[row, i]);
        }

        return true;
    }

    private bool CheckForVerticalWin(TicTacToeBoard boardToCheck)
    {
        if (boardToCheck.WinnerState == WinnerOptions.NEITHER) return false;
        // Check for Column Win
        int column = boardToCheck.Coordinates.y;
        for (int i = 0; i < boards.GetLength(0); i++)
        {
            TicTacToeBoard currentlyChecking = boards[i, column];
            if (currentlyChecking.WinnerState != boardToCheck.WinnerState)
            {
                return false;
            }
        }

        // if we are here, that means a vertical win has occured
        for (int i = 0; i < boards.GetLength(0); i++)
        {
            winBoardList.Add(boards[i, column]);
        }
        return true;
    }

    private bool CheckForDiagonalWin(TicTacToeBoard boardToCheck)
    {
        if (boardToCheck.WinnerState == WinnerOptions.NEITHER) return false;
        bool won = true;
        //
        for (int i = 0; i < boards.GetLength(0); i++)
        {
            TicTacToeBoard currentlyChecking = boards[i, i];
            if (currentlyChecking.WinnerState != boardToCheck.WinnerState)
            {
                won = false;
                break;
            }
        }
        if (won)
        {
            // if we are here, that means a diagonal win in the top left -> bottom right orientationhas occured
            for (int i = 0; i < boards.GetLength(0); i++)
            {
                winBoardList.Add(boards[i, i]);
            }
            return true;
        }

        won = true;
        // 
        for (int i = 0; i < boards.GetLength(0); i++)
        {
            TicTacToeBoard currentlyChecking = boards[i, boards.GetLength(0) - 1 - i];
            if (currentlyChecking.WinnerState != boardToCheck.WinnerState)
            {
                won = false;
                break;
            }
        }
        if (won)
        {
            // if we are here, that means a diagonal win in the top left -> bottom right orientationhas occured
            for (int i = 0; i < boards.GetLength(0); i++)
            {
                winBoardList.Add(boards[i, boards.GetLength(0) - 1 - i]);
            }
            return true;
        }
        return false;
    }

    private void CheckForTie()
    {
        for (int i = 0; i < boards.GetLength(0); i++)
        {
            for (int p = 0; p < boards.GetLongLength(0); p++)
            {
                // Empty cells exist, therefore not at a stalemate yet
                TicTacToeBoard checkingBoard = boards[i, p];
                if (!checkingBoard.GameWon && checkingBoard.HasEmptySpace())
                {
                    return;
                }
            }
        }
        // Successfully looped through all boards without coming across an empty cell, therefore game is tied
        GameTied = true;
    }

    public IEnumerator CheckMoveResult(TwoPlayerGameState moveOccurredOn, TicTacToeBoardCell alteredCell)
    {
        yield return StartCoroutine(alteredCell.OwnerOfCell.CheckMoveResult(moveOccurredOn, alteredCell));

        if (alteredCell.OwnerOfCell.GameWon)
        {
            yield return new WaitForSeconds(delayAfterBoardWin);
        }

        CheckForWin(alteredCell.OwnerOfCell);
        CheckForTie();

        if (!GameWon)
        {
            TicTacToeBoard checkingBoard = boards[alteredCell.Coordinates.x, alteredCell.Coordinates.y];
            if (checkingBoard.HasEmptySpace() && !checkingBoard.GameWon)
            {
                for (int i = 0; i < boards.GetLength(0); i++)
                {
                    for (int p = 0; p < boards.GetLongLength(0); p++)
                    {
                        boards[i, p].SetInteractable(i == alteredCell.Coordinates.x && p == alteredCell.Coordinates.y);
                    }
                }
            }
            else
            {
                for (int i = 0; i < boards.GetLength(0); i++)
                {
                    for (int p = 0; p < boards.GetLongLength(0); p++)
                    {
                        boards[i, p].SetInteractable(true);
                    }
                }
            }
        }
        else
        {
            SetWinnerState(moveOccurredOn == TwoPlayerGameState.P1 ? WinnerOptions.P1 : WinnerOptions.P2);

            yield return StartCoroutine(AnimateWinningBoards());
        }

        HasChanged = true;
    }

    private void CheckForWin(TicTacToeBoard boardToCheck)
    {
        if (CheckForHorizontalWin(boardToCheck))
        {
            GameWon = true;
            return;
        };
        if (CheckForVerticalWin(boardToCheck))
        {
            GameWon = true;
            return;
        };
        if (CheckForDiagonalWin(boardToCheck))
        {
            GameWon = true;
            return;
        };
    }

    public void SetWinnerState(WinnerOptions winnerState)
    {
        WinnerState = winnerState;
    }

    public IEnumerator AnimateWinningBoards()
    {
        float pitchChange = 0;
        foreach (TicTacToeBoard board in winBoardList)
        {
            StartCoroutine(board.ChangeCoverColor(TwoPlayerDataDealer._Instance.GetWinCellColor()));

            // Audio
            AudioManager._Instance.PlayFromSFXDict(onRevealWinningBoard, pitchChange);
            pitchChange += pitchIncreasePerCell;

            yield return new WaitForSeconds(delayBetweenWinBoardAnimations);
        }

        // Audio
        AudioManager._Instance.PlayFromSFXDict(onGameWon);
    }

    [ContextMenu("FillBoard")]
    public void FillBoard()
    {
        ActOnEachBoardCell(cell =>
        {
            cell.HardSetToSymbol(TwoPlayerCellState.X);
        });
        CheckForTie();
    }
}
