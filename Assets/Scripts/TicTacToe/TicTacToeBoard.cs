using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TicTacToeBoard : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private float delayBetweenWinCellAnimations = 0.1f;
    private List<TicTacToeBoardCell> winCellList;
    private TicTacToeBoardCell[,] board;

    [Header("Visual & References")]
    [SerializeField] private TicTacToeBoardCell boardCellPrefab;
    [SerializeField] private Transform parentCellSpawnsTo;
    [SerializeField] private GridLayoutGroup glGroup;
    [SerializeField] private int cellSize;
    [SerializeField] private float delayBetweenCellSpawns = 0.0f;
    [SerializeField] private float pitchIncreasePerCell;
    [SerializeField] private float glGroupSpacing = 5.0f;
    [SerializeField] private RectTransform mainTransform;
    [SerializeField] private CanvasGroup mainCanvasGroup;
    [SerializeField] private float coverImageAlphaGainRate = 5.0f;
    [SerializeField] private float changeAlphaRate = 5.0f;
    [SerializeField] private float changeColorRate = 5.0f;
    [SerializeField] private float changeScaleRate = 5.0f;

    [Header("Audio")]
    [SerializeField] private string onCellChanged = "tttBoard_onCellChanged";
    [SerializeField] private string onGameWon = "tttBoard_onGameWon";
    [SerializeField] private string onCellSpawned = "tttBoard_onCellSpawned";
    [SerializeField] private string onFinishedSpawningCells = "tttBoard_onFinishedSpawningCells";
    [SerializeField] private string onRevealWinningCell = "tttBoard_onRevealWinningCell";

    private bool useCoverImageOnWin;

    public Vector2Int Coordinates;

    public bool HasChanged { get; private set; }
    public bool GameWon { get; private set; }
    public bool GameTied { get; private set; }
    public WinnerOptions WinnerState { get; private set; }
    private bool interactable;

    [SerializeField] private Image coverImage;
    private CanvasGroup coverImageCanvasGroup;

    public void SetInteractable(bool v)
    {
        interactable = v;
        foreach (TicTacToeBoardCell cell in board)
        {
            cell.SetInteractable(interactable);
        }
    }

    public void SetWinnerState(WinnerOptions winnerState)
    {
        WinnerState = winnerState;
    }

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
            if (cell.GetState() == TicTacToeBoardCellState.NULL)
            {
                cell.GetSymbolImage().sprite = s;
            }
        });
    }

    public void ResetHasChanged()
    {
        HasChanged = false;
    }

    public IEnumerator Generate(int boardSize, bool useCoverImageOnWin)
    {
        this.useCoverImageOnWin = useCoverImageOnWin;
        GameWon = false;
        GameTied = false;
        HasChanged = false;
        winCellList = new List<TicTacToeBoardCell>();

        glGroup.cellSize = new Vector2(cellSize, cellSize);
        glGroup.constraintCount = boardSize;
        glGroup.spacing = glGroupSpacing * Vector2.one;
        glGroup.GetComponent<RectTransform>().sizeDelta = Vector2.one * boardSize * cellSize;

        coverImageCanvasGroup = coverImage.GetComponent<CanvasGroup>();

        board = new TicTacToeBoardCell[boardSize, boardSize];
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLongLength(0); p++)
            {
                TicTacToeBoardCell cell = Instantiate(boardCellPrefab, parentCellSpawnsTo);
                cell.Coordinates = new Vector2Int(i, p);
                cell.OwnerOfCell = this;
                board[i, p] = cell;
                cell.name += "<" + i + ", " + p + ">";
            }
        }

        yield return null;

        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLongLength(0); p++)
            {
                AudioManager._Instance.PlayFromSFXDict(onCellSpawned);

                TicTacToeBoardCell cell = board[i, p];

                StartCoroutine(cell.ChangeScale(.9f));
                if (i == board.GetLongLength(0) - 1 && p == board.GetLength(0) - 1)
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
                if (board[i, p].GetState() == TicTacToeBoardCellState.NULL)
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

    public IEnumerator CheckMoveResult(TicTacToeGameState moveOccurredOn, TicTacToeBoardCell alteredCell)
    {
        AudioManager._Instance.PlayFromSFXDict(onCellChanged);

        CheckForWin(alteredCell);
        CheckForTie();

        if (GameWon)
        {
            yield return StartCoroutine(AnimateWinningCells());

            SetWinnerState(moveOccurredOn == TicTacToeGameState.P1 ? WinnerOptions.P1 : WinnerOptions.P2);

            if (useCoverImageOnWin)
            {
                StartCoroutine(ChangeCoverAlpha(1));
                yield return StartCoroutine(ChangeCoverColor(TicTacToeDataDealer._Instance.GetPlayerColor(WinnerState)));
            }
        }

        HasChanged = true;
    }

    public bool HasEmptySpace()
    {
        foreach (TicTacToeBoardCell cell in board)
        {
            if (cell.GetState() == TicTacToeBoardCellState.NULL) return true;
        }
        return false;
    }

    public IEnumerator AnimateWinningCells()
    {
        // Hide all Symbols
        foreach (TicTacToeBoardCell cell in board)
        {
            StartCoroutine(cell.LockSymbolAlpha(0));
        }

        float pitchChange = 0;
        foreach (TicTacToeBoardCell cell in winCellList)
        {
            cell.SetCoverColor(TicTacToeDataDealer._Instance.GetWinCellColor());
            StartCoroutine(cell.ChangeCoverAlpha(1));

            // Audio
            AudioManager._Instance.PlayFromSFXDict(onRevealWinningCell, pitchChange);

            pitchChange += pitchIncreasePerCell;

            yield return new WaitForSeconds(delayBetweenWinCellAnimations);
        }

        // Audio
        AudioManager._Instance.PlayFromSFXDict(onGameWon);
    }

    public IEnumerator ChangeCoverAlpha(float target)
    {
        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(coverImageCanvasGroup, target, coverImageAlphaGainRate));
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

    public IEnumerator ChangeCoverColor(Color target)
    {
        yield return StartCoroutine(Utils.ChangeColor(coverImage, target, changeColorRate));
    }
}
