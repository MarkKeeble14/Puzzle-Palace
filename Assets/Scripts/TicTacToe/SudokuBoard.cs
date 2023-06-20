using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class SudokuBoard : MonoBehaviour
{
    [Header("Data")]
    private SudokuBoardCell[,] board;

    [Header("Visual & References")]
    [SerializeField] private SudokuBoardCell boardCellPrefab;
    [SerializeField] private Transform parentCellSpawnsTo;
    [SerializeField] private float delayBetweenCellSpawns = 0.0f;
    [SerializeField] private float pitchIncreasePerCell;
    [SerializeField] private RectTransform mainTransform;
    [SerializeField] private CanvasGroup mainCanvasGroup;
    [SerializeField] private float changeAlphaRate = 5.0f;
    [SerializeField] private float changeScaleRate = 5.0f;

    [Header("Audio")]
    [SerializeField] private string onCellSpawned = "tttBoard_onCellSpawned";
    [SerializeField] private string onFinishedSpawningCells = "tttBoard_onFinishedSpawningCells";

    [SerializeField] private GameObject innerGridPrefab;
    [SerializeField] private Transform[,] innerGrids;

    [SerializeField] private Color evenInnerGridColor;
    [SerializeField] private Color oddInnerGridColor;


    public bool HasChanged { get; private set; }
    public bool GameWon { get; private set; }
    public bool GameTied { get; private set; }
    public WinnerOptions WinnerState { get; private set; }
    private bool interactable;

    public void SetInteractable(bool v)
    {
        interactable = v;
        foreach (SudokuBoardCell cell in board)
        {
            cell.SetInteractable(interactable);
        }
    }

    public void ActOnEachBoardCell(Action<SudokuBoardCell> func)
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLength(1); p++)
            {
                func(board[i, p]);
            }
        }
    }

    public IEnumerator ActOnEachBoardCellWithDelay(Action<SudokuBoardCell> func, float delay, bool reverseOrder)
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

    internal void UnshowCellsWithChar()
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLength(1); p++)
            {
                board[i, p].SetSymbolAlpha(1);

                board[i, p].SetAllPencilledCharAlpha(0);
            }
        }
    }

    public void ShowCellsWithChar(char v)
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLength(1); p++)
            {
                board[i, p].TrySetPencilledCharAlpha(v, .5f);

                if (board[i, p].GetInputtedChar().Equals(v))
                {
                    board[i, p].SetSymbolAlpha(.6f);
                }
            }
        }
    }

    public void ResetHasChanged()
    {
        HasChanged = false;
    }

    private Transform GetCorrespondingGrid(int row, int col)
    {
        int boxStartRow = row - (row % 3);
        int boxStartCol = col - (col % 3);

        // Debug.Log(row + ", " + col + " - " + boxStartRow + ", " + boxStartCol);

        if (boxStartRow == 0 && boxStartCol == 0) return innerGrids[0, 0];
        if (boxStartRow == 0 && boxStartCol == 3) return innerGrids[0, 1];
        if (boxStartRow == 0 && boxStartCol == 6) return innerGrids[0, 2];
        if (boxStartRow == 3 && boxStartCol == 0) return innerGrids[1, 0];
        if (boxStartRow == 3 && boxStartCol == 3) return innerGrids[1, 1];
        if (boxStartRow == 3 && boxStartCol == 6) return innerGrids[1, 2];
        if (boxStartRow == 6 && boxStartCol == 0) return innerGrids[2, 0];
        if (boxStartRow == 6 && boxStartCol == 3) return innerGrids[2, 1];
        if (boxStartRow == 6 && boxStartCol == 6) return innerGrids[2, 2];
        return null;
    }

    public IEnumerator Generate(Action<SudokuBoardCell> onPressCellAction, List<int> acceptedNums, Vector2Int minMaxNumHoles)
    {
        innerGrids = new Transform[3, 3];
        for (int i = 0; i < innerGrids.GetLength(0); i++)
        {
            for (int p = 0; p < innerGrids.GetLength(1); p++)
            {
                innerGrids[i, p] = Instantiate(innerGridPrefab, parentCellSpawnsTo).transform;
                innerGrids[i, p].name += "<" + i + ", " + p + ">";
            }
        }

        board = new SudokuBoardCell[9, 9];
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLength(1); p++)
            {
                // Debug.Log(i + ", " + p);
                SudokuBoardCell cell = Instantiate(boardCellPrefab, GetCorrespondingGrid(i, p));
                cell.SetInteractable(false);
                cell.AddOnPressedAction(onPressCellAction);
                cell.Coordinates = new Vector2Int(i, p);
                cell.SetCorrectChar('0');
                board[i, p] = cell;
                cell.name += "<" + i + ", " + p + ">";
            }
        }

        for (int i = 0; i < innerGrids.GetLength(0); i++)
        {
            for (int p = 0; p < innerGrids.GetLength(1); p++)
            {
                foreach (SudokuBoardCell cell in innerGrids[i, p].GetComponentsInChildren<SudokuBoardCell>())
                {
                    cell.SetSymbolColor((i + p) % 2 == 0 ? evenInnerGridColor : oddInnerGridColor);
                }
            }
        }

        PopulateBoard(acceptedNums);

        PokeHoles(RandomHelper.RandomIntExclusive(minMaxNumHoles));

        SolidifyBoard();

        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLength(1); p++)
            {
                AudioManager._Instance.PlayFromSFXDict(onCellSpawned);

                SudokuBoardCell cell = board[i, p];

                if (i == board.GetLength(0) - 1 && p == board.GetLength(1) - 1)
                {
                    yield return StartCoroutine(cell.ChangeScale(.9f));
                    SetInteractable(true);
                }
                else
                {
                    StartCoroutine(cell.ChangeScale(.9f));
                }

                yield return new WaitForSeconds(delayBetweenCellSpawns);
            }
        }

        AudioManager._Instance.PlayFromSFXDict(onFinishedSpawningCells);
    }

    public void FullyPencilInUnfilled(List<int> allowedNums)
    {
        ActOnEachBoardCell(cell =>
        {
            if (!cell.GetInputtedChar().Equals(' '))
            {
                return;
            }

            for (int i = 0; i < allowedNums.Count; i++)
            {
                cell.PencilChar(allowedNums[i].ToString()[0]);
            }
        });
    }

    public void CorrectlyPencilInUnfilled(List<int> allowedNums)
    {
        ActOnEachBoardCell(cell =>
        {
            if (!cell.GetInputtedChar().Equals(' '))
            {
                return;
            }

            for (int i = 0; i < allowedNums.Count; i++)
            {
                char c = allowedNums[i].ToString()[0];

                if (!CheckIfRowContainsChar(c, cell.Coordinates.x)
                && !CheckIfColContainsChar(c, cell.Coordinates.y)
                && !CheckIfRegionContainsChar(c, cell.Coordinates.x, cell.Coordinates.y))
                {
                    cell.PencilChar(c);
                }
                else
                {
                    cell.TryRemovePencilChar(c);
                }

            }
        });
    }

    private void SolidifyBoard()
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLength(1); p++)
            {
                if (!board[i, p].GetInputtedChar().Equals(' '))
                {
                    board[i, p].Lock();
                }
            }
        }
    }

    public void RemoveCharFromInvalidLocations(SudokuBoardCell selectedCell, char v)
    {
        RemoveCharFromRow(v, selectedCell.Coordinates.x);
        RemoveCharFromCol(v, selectedCell.Coordinates.y);
        RemoveCharFromRegion(v, selectedCell.Coordinates.x, selectedCell.Coordinates.y);
    }

    public SudokuBoardCell CheckIfRowContainsChar(char v, int rowIndex)
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            if (board[rowIndex, i].GetInputtedChar().Equals(v))
                return board[rowIndex, i];
        }
        return null;
    }

    public SudokuBoardCell CheckIfColContainsChar(char v, int colIndex)
    {
        for (int i = 0; i < board.GetLength(1); i++)
        {
            if (board[i, colIndex].GetInputtedChar().Equals(v))
                return board[i, colIndex];
        }
        return null;
    }

    public SudokuBoardCell CheckIfRegionContainsChar(char v, int rowIndex, int colIndex)
    {
        int boxStartRow = rowIndex - (rowIndex % 3);
        int boxStartCol = colIndex - (colIndex % 3);
        for (int i = 0; i < 3; i++)
        {
            for (int p = 0; p < 3; p++)
            {
                if (board[boxStartRow + i, boxStartCol + p].GetInputtedChar().Equals(v))
                    return board[boxStartRow + i, boxStartCol + p];
            }
        }
        return null;
    }

    private void RemoveCharFromRow(char v, int rowIndex)
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            board[rowIndex, i].TryRemovePencilChar(v);
        }
    }

    private void RemoveCharFromCol(char v, int colIndex)
    {
        for (int i = 0; i < board.GetLength(1); i++)
        {
            board[i, colIndex].TryRemovePencilChar(v);
        }
    }

    private void RemoveCharFromRegion(char v, int rowIndex, int colIndex)
    {
        int boxStartRow = rowIndex - (rowIndex % 3);
        int boxStartCol = colIndex - (colIndex % 3);
        for (int i = 0; i < 3; i++)
        {
            for (int p = 0; p < 3; p++)
            {
                board[boxStartRow + i, boxStartCol + p].TryRemovePencilChar(v);
            }
        }
    }

    private bool PopulateBoard(List<int> acceptedNums)
    {
        SudokuBoardCell cell = FindEmptyCell();
        // Debug.Log("Populating Cell: " + cell);

        // if there are no empty cells, the board is complete
        if (!cell) return true;

        acceptedNums.Shuffle();

        for (int i = 0; i < acceptedNums.Count; i++)
        {
            int num = acceptedNums[i];
            // Debug.Log("Attempting to Place: " + num + " - into: " + cell);

            cell.SetCorrectChar(num.ToString()[0]);
            cell.SetInputtedChar(num.ToString()[0]);

            if (CheckForCellValidity(cell))
            {
                // Debug.Log("1. Next: " + cell);
                // PrintBoardState();
                // Debug.Log("Placement Valid");
                if (PopulateBoard(acceptedNums))
                {
                    //Debug.Log("3. Success: " + cell);
                    // Debug.Log("Board Population Successful");
                    return true;
                }

                // Debug.Log("Board Population Unsuccessful; Future Placement Was Invalid");
                cell.SetCorrectChar('0');
                cell.SetInputtedChar('0');
                // PrintBoardState();
            }
        }

        cell.SetCorrectChar('0');
        cell.SetInputtedChar('0');
        // Debug.Log("2. Back: " + cell);
        // Debug.Log("Board Population Unsuccessful; No Acceptable Nums");
        return false;
    }

    private void PrintBoardState()
    {
        string s = "";
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLength(1); p++)
            {
                s += board[i, p].GetCorrectChar();
                if (p < board.GetLength(1) - 1)
                {
                    s += " ";
                }
            }
            s += "\n";
        }
        Debug.Log(s);
    }

    private void PokeHoles(int numHoles)
    {
        int[,] removedVals = new int[board.GetLength(0), board.GetLength(1)];
        int numValsRemoved = 0;

        List<int> indiciesCanRemove = new List<int>();
        indiciesCanRemove.AddRange(Enumerable.Range(0, 81));
        indiciesCanRemove.Shuffle();

        while (numValsRemoved < numHoles)
        {
            int nextVal = indiciesCanRemove[0];
            indiciesCanRemove.Remove(nextVal);

            if (indiciesCanRemove.Count <= 0)
            {
                throw new Exception("Impossible Game Occurred While Poking Holes");
            }

            int rowIndex = Mathf.FloorToInt(nextVal / 9);
            int colIndex = nextVal % 9;

            if (board[rowIndex, colIndex].GetCorrectChar().Equals('0'))
            {
                // Already poked a hole here
                continue;
            }

            removedVals[rowIndex, colIndex] = board[rowIndex, colIndex].GetCorrectChar();
            board[rowIndex, colIndex].SetInputtedChar(' ');
            numValsRemoved++;

            if (MultipleSolutionsExist())
            {
                board[rowIndex, colIndex].SetInputtedChar(removedVals[rowIndex, colIndex].ToString()[0]);
                removedVals[rowIndex, colIndex] = '0';
                numValsRemoved--;
            }
        }
    }

    private bool MultipleSolutionsExist()
    {
        // TODO
        return false;
    }


    private SudokuBoardCell FindEmptyCell()
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLength(1); p++)
            {
                // Debug.Log(i + ", " + p);
                SudokuBoardCell cell = board[i, p];

                if (cell.GetCorrectChar().Equals('0'))
                {
                    return cell;
                }
            }
        }
        return null;
    }

    private bool CheckBoardForValidity()
    {
        foreach (SudokuBoardCell cell in board)
        {
            if (!CheckForCellValidity(cell))
            {
                return false;
            }
        }
        return true;
    }

    public void CheatBoard()
    {
        foreach (SudokuBoardCell cell in board)
        {
            cell.SetInputtedChar(cell.GetCorrectChar());
        }
    }

    private bool CheckForCellValidity(SudokuBoardCell cell)
    {
        return RowSafe(cell)
            && ColSafe(cell)
            && RegionSafe(cell);
    }

    private bool RowSafe(SudokuBoardCell cell)
    {
        for (int i = 0; i < board.GetLength(1); i++)
        {
            SudokuBoardCell checkCell = board[cell.Coordinates.x, i];

            if (checkCell.Equals(cell)) continue;

            if (checkCell.GetCorrectChar().Equals(cell.GetCorrectChar()))
            {
                // Debug.Log("Row Invalid - Current Cell: " + cell + ", Checking: " + checkCell +
                //    " Already Contains: " + cell.GetCorrectChar());
                return false;
            }
        }
        // Debug.Log("Cell: " + cell + ", With Placement of: " + cell.GetCorrectChar() + " - Row Valid");
        return true;
    }

    private bool ColSafe(SudokuBoardCell cell)
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            SudokuBoardCell checkCell = board[i, cell.Coordinates.y];

            if (checkCell.Equals(cell)) continue;

            if (checkCell.GetCorrectChar().Equals(cell.GetCorrectChar()))
            {
                // Debug.Log("Col Invalid - Current Cell: " + cell + ", Checking: " + checkCell +
                //    " Already Contains: " + cell.GetCorrectChar());
                return false;
            }
        }
        // Debug.Log("Cell: " + cell + ", With Placement of: " + cell.GetCorrectChar() + " - Col Valid");
        return true;
    }

    private bool RegionSafe(SudokuBoardCell cell)
    {
        int boxStartRow = cell.Coordinates.x - (cell.Coordinates.x % 3);
        int boxStartCol = cell.Coordinates.y - (cell.Coordinates.y % 3);
        for (int i = 0; i < 3; i++)
        {
            for (int p = 0; p < 3; p++)
            {
                SudokuBoardCell checkCell = board[boxStartRow + i, boxStartCol + p];

                if (checkCell.Equals(cell)) continue;

                if (checkCell.GetCorrectChar().Equals(cell.GetCorrectChar()))
                {
                    // Debug.Log("Region Invalid - Current Cell: " + cell + ", Checking: " + checkCell +
                    //    " Already Contains: " + cell.GetCorrectChar());
                    return false;
                }
            }
        }
        // Debug.Log("Cell: " + cell + ", With Placement of: " + cell.GetCorrectChar() + " - Region Valid");
        return true;
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

    public bool CheckForWin()
    {
        foreach (SudokuBoardCell cell in board)
        {
            if (!cell.HasCorrectChar()) return false;
        }
        return true;
    }
}
