using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectFourBoard : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private float delayBetweenWinCellAnimations = 0.1f;
    private List<ConnectFourBoardCell> winCellList;
    private ConnectFourBoardCell[,] board;

    [Header("Visual & References")]
    [SerializeField] private ConnectFourBoardCell boardCellPrefab;
    [SerializeField] private Transform parentCellSpawnsTo;
    [SerializeField] private GridLayoutGroup glGroup;
    [SerializeField] private int cellSize;
    [SerializeField] private float delayBetweenCellSpawns = 0.0f;
    [SerializeField] private float pitchIncreasePerCell;
    [SerializeField] private float glGroupSpacing = 5.0f;
    [SerializeField] private RectTransform mainTransform;
    [SerializeField] private CanvasGroup mainCanvasGroup;
    [SerializeField] private float changeAlphaRate = 5.0f;
    [SerializeField] private float changeScaleRate = 5.0f;

    [Header("Audio")]
    [SerializeField] private string onCellChanged = "tttBoard_onCellChanged";
    [SerializeField] private string onGameWon = "tttBoard_onGameWon";
    [SerializeField] private string onCellSpawned = "tttBoard_onCellSpawned";
    [SerializeField] private string onFinishedSpawningCells = "tttBoard_onFinishedSpawningCells";
    [SerializeField] private string onRevealWinningCell = "tttBoard_onRevealWinningCell";

    public bool HasChanged { get; private set; }
    public bool GameWon { get; private set; }
    public bool GameTied { get; private set; }
    public WinnerOptions WinnerState { get; private set; }
    private bool interactable;
    private ConnectFourBoardCell checkingCell;

    public void SetInteractable(bool v)
    {
        interactable = v;
        foreach (ConnectFourBoardCell cell in board)
        {
            cell.SetInteractable(interactable);
        }
    }

    public void SetWinnerState(WinnerOptions winnerState)
    {
        WinnerState = winnerState;
    }

    public void ActOnEachBoardCell(Action<ConnectFourBoardCell> func)
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLength(1); p++)
            {
                func(board[i, p]);
            }
        }
    }

    public IEnumerator ActOnEachBoardCellWithDelay(Action<ConnectFourBoardCell> func, float delay, bool reverseOrder)
    {
        if (reverseOrder)
        {
            for (int i = board.GetLength(0) - 1; i >= 0; i--)
            {
                for (int p = (int)board.GetLength(1) - 1; p >= 0; p--)
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
                for (int p = 0; p < board.GetLength(1); p++)
                {
                    func(board[i, p]);
                    yield return new WaitForSeconds(delay);
                }
            }
        }
    }

    public void ResetHasChanged()
    {
        HasChanged = false;
    }

    public IEnumerator Generate(Vector2Int boardSize)
    {
        GameWon = false;
        GameTied = false;
        HasChanged = false;
        winCellList = new List<ConnectFourBoardCell>();

        glGroup.cellSize = new Vector2(cellSize, cellSize);
        glGroup.constraintCount = boardSize.y;
        glGroup.spacing = glGroupSpacing * Vector2.one;
        glGroup.GetComponent<RectTransform>().sizeDelta = Vector2.one * boardSize * cellSize;

        board = new ConnectFourBoardCell[boardSize.x, boardSize.y];
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLength(1); p++)
            {
                // Debug.Log(i + ", " + p);
                ConnectFourBoardCell cell = Instantiate(boardCellPrefab, parentCellSpawnsTo);
                cell.Coordinates = new Vector2Int(i, p);
                board[i, p] = cell;
                cell.name += "<" + i + ", " + p + ">";
            }
        }

        yield return null;

        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLength(1); p++)
            {
                AudioManager._Instance.PlayFromSFXDict(onCellSpawned);

                ConnectFourBoardCell cell = board[i, p];

                StartCoroutine(cell.ChangeScale(.9f));
                if (i == board.GetLength(0) - 1 && p == board.GetLength(1) - 1)
                {
                    yield return StartCoroutine(cell.ChangeTotalAlpha(1));
                    SetInteractable(true);
                }
                else
                {
                    StartCoroutine(cell.ChangeTotalAlpha(1));
                }

                yield return new WaitForSeconds(delayBetweenCellSpawns);
            }
        }

        AudioManager._Instance.PlayFromSFXDict(onFinishedSpawningCells);
    }

    public IEnumerator ShowMove(ConnectFourBoardCell alteredCell, TwoPlayerCellState state)
    {
        AudioManager._Instance.PlayFromSFXDict(onCellChanged);

        Vector2Int check = new Vector2Int(0, alteredCell.Coordinates.y);
        board[check.x, check.y].HardSetToSymbol(state);

        while (check.x + 1 < board.GetLength(0) &&
            board[check.x + 1, check.y].GetState() == TwoPlayerCellState.NULL)
        {
            yield return new WaitForSeconds(.1f);
            board[check.x, check.y].HardSetToSymbol(TwoPlayerCellState.NULL);
            check.x++;
            board[check.x, check.y].HardSetToSymbol(state);
        }

        checkingCell = board[check.x, check.y];
    }

    private bool CheckForHorizontalWin()
    {
        // Check for Row Win
        int count = 0;
        int row = checkingCell.Coordinates.x;
        for (int col = 0; col < board.GetLength(1); col++)
        {
            ConnectFourBoardCell currentlyChecking = board[row, col];
            if (currentlyChecking.GetState() != checkingCell.GetState())
            {
                winCellList.Clear();
                count = 0;
            }
            else
            {
                count++;
                winCellList.Add(board[row, col]);
                if (count >= 4)
                {
                    return true;
                }
            }
        }
        winCellList.Clear();
        return false;
    }

    private bool CheckForVerticalWin()
    {
        // Check for Column Win
        int count = 0;
        int column = checkingCell.Coordinates.y;
        for (int i = 0; i < board.GetLength(0); i++)
        {
            ConnectFourBoardCell currentlyChecking = board[i, column];
            if (currentlyChecking.GetState() != checkingCell.GetState())
            {
                winCellList.Clear();
                count = 0;
            }
            else
            {
                count++;
                winCellList.Add(board[i, column]);
                if (count >= 4)
                {
                    return true;
                }
            }
        }
        winCellList.Clear();
        return false;
    }

    private bool CheckForDiagonalWin()
    {
        // Debug.Log("0: " + board.GetLength(0) + ", 1: " + board.GetLength(1));
        Vector2Int check = checkingCell.Coordinates;

        return
                CheckForDiagonalWinHelper(check, -1, -1)
            || CheckForDiagonalWinHelper(check, -1, 1)
            || CheckForDiagonalWinHelper(check, 1, -1)
            || CheckForDiagonalWinHelper(check, 1, 1);
    }

    private bool CheckForDiagonalWinHelper(Vector2Int check, int xChange, int yChange)
    {
        int count = 0;

        while (check.x < board.GetLength(0) - 1 && check.x > 0
            && check.y < board.GetLength(1) - 1 && check.y > 0)
        {
            check.x -= xChange;
            check.y -= yChange;
        }

        ConnectFourBoardCell currentlyChecking = board[check.x, check.y];

        while (currentlyChecking != null)
        {
            // Debug.Log(currentlyChecking.Coordinates + ", " + xChange + ", " + yChange);
            if (currentlyChecking.GetState() != checkingCell.GetState())
            {
                // Debug.Log("Breaking");
                break;
            }
            else
            {
                // Debug.Log("Found Another in Sequence");
                count++;
                winCellList.Add(currentlyChecking);

                if (count >= 4)
                {
                    return true;
                }

                check.x += xChange;
                check.y += yChange;
                // Debug.Log("X: " + check.x + ", Y: " + check.y);
                if (check.x > board.GetLength(0) - 1 || check.x < 0 || check.y > board.GetLength(1) - 1 || check.y < 0)
                {
                    break;
                }
                else
                {
                    currentlyChecking = board[check.x, check.y];
                }
            }

        }
        winCellList.Clear();
        return false;
    }

    private void CheckForTie()
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLength(1); p++)
            {
                // Empty cells exist, therefore not at a stalemate yet
                if (board[i, p].GetState() == TwoPlayerCellState.NULL)
                {
                    return;
                }
            }
        }
        // Successfully looped through all cells without coming across an empty cell, therefore game is tied
        GameTied = true;
        return;
    }

    private void CheckForWin()
    {
        List<ConnectFourBoardCell> rememberWinCells = new List<ConnectFourBoardCell>();
        if (CheckForHorizontalWin())
        {
            // Debug.Log("Horizontal Win");
            GameWon = true;
            rememberWinCells.AddRange(winCellList);
        };
        if (CheckForVerticalWin())
        {
            // Debug.Log("Vertical Win");
            GameWon = true;
            rememberWinCells.AddRange(winCellList);
        };
        if (CheckForDiagonalWin())
        {
            // Debug.Log("Diagonal Win");
            GameWon = true;
            rememberWinCells.AddRange(winCellList);
        };
        winCellList.Clear();
        foreach (ConnectFourBoardCell cell in rememberWinCells)
        {
            if (!winCellList.Contains(cell))
            {
                winCellList.Add(cell);
            }
        }
    }

    public IEnumerator CheckMoveResult(TwoPlayerGameState moveOccurredOn, ConnectFourBoardCell alteredCell)
    {
        CheckForWin();
        CheckForTie();

        if (GameWon)
        {
            yield return StartCoroutine(AnimateWinningCells());

            SetWinnerState(moveOccurredOn == TwoPlayerGameState.P1 ? WinnerOptions.P1 : WinnerOptions.P2);
        }

        HasChanged = true;
    }

    public bool HasEmptySpace()
    {
        foreach (ConnectFourBoardCell cell in board)
        {
            if (cell.GetState() == TwoPlayerCellState.NULL) return true;
        }
        return false;
    }

    public IEnumerator AnimateWinningCells()
    {
        // Hide all Symbols
        // foreach (ConnectFourBoardCell cell in board)
        // {
        //    StartCoroutine(cell.LockSymbolAlpha(0));
        // }

        float pitchChange = 0;
        foreach (ConnectFourBoardCell cell in winCellList)
        {
            cell.SetCoverColor(TwoPlayerDataDealer._Instance.GetWinCellColor());
            StartCoroutine(cell.ChangeCoverAlpha(1));

            // Audio
            AudioManager._Instance.PlayFromSFXDict(onRevealWinningCell, pitchChange);

            pitchChange += pitchIncreasePerCell;

            yield return new WaitForSeconds(delayBetweenWinCellAnimations);
        }

        // Audio
        AudioManager._Instance.PlayFromSFXDict(onGameWon);
    }

    public IEnumerator ChangeScale(Vector3 target)
    {
        yield return StartCoroutine(Utils.ChangeScale(mainTransform, target, changeScaleRate));
    }

    public IEnumerator ChangeScale(float target)
    {
        yield return StartCoroutine(ChangeScale(new Vector3(target, target, target)));
    }

    public IEnumerator ChangeTotalAlpha(float target)
    {
        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(mainCanvasGroup, target, changeAlphaRate));
    }
}
