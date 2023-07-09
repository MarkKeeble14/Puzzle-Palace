using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OperationsBoard : MonoBehaviour
{
    [Header("Data")]
    private OperationsBoardCell[,] board;

    [Header("Visual & References")]
    [SerializeField] private OperationsBoardCell boardCellPrefab;
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

    public bool HasChanged { get; private set; }
    public bool GameWon { get; private set; }
    public bool GameTied { get; private set; }
    public WinnerOptions WinnerState { get; private set; }
    private bool interactable;

    public void SetInteractable(bool v)
    {
        interactable = v;
        foreach (OperationsBoardCell cell in board)
        {
            cell.SetInteractable(interactable);
        }
    }

    public void ActOnEachBoardCell(Action<OperationsBoardCell> func)
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLength(1); p++)
            {
                func(board[i, p]);
            }
        }
    }

    public IEnumerator ActOnEachBoardCellWithDelay(Action<OperationsBoardCell> func, float delay, bool reverseOrder)
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
                    board[i, p].SetSymbolAlpha(.75f);
                }
            }
        }
    }

    public void ResetHasChanged()
    {
        HasChanged = false;
    }

    public IEnumerator Generate(Action<OperationsBoardCell> onPressCellAction, List<int> acceptedNums, List<MathematicalOperation> acceptedOps, SerializableDictionary<MathematicalOperation, int> maxOpsDict)
    {
        board = new OperationsBoardCell[6, 6];
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLength(1); p++)
            {
                // Debug.Log(i + ", " + p);
                OperationsBoardCell cell = Instantiate(boardCellPrefab, parentCellSpawnsTo);
                cell.SetInteractable(false);
                cell.AddOnPressedAction(onPressCellAction);
                cell.Coordinates = new Vector2Int(i, p);
                cell.SetCorrectChar('0');
                cell.SetInputtedChar(' ');
                board[i, p] = cell;
                cell.name += "<" + i + ", " + p + ">";
            }
        }

        List<int> populateBoardWith = new List<int>();
        populateBoardWith.AddRange(acceptedNums);
        populateBoardWith.Shuffle();

        // Set Types of Cells according to the formats defined below
        List<OperationsBoardCellType> evenFormat = new List<OperationsBoardCellType>() {
            OperationsBoardCellType.NUM, OperationsBoardCellType.OP, OperationsBoardCellType.NUM, OperationsBoardCellType.OP, OperationsBoardCellType.NUM, OperationsBoardCellType.ANSWER };
        List<OperationsBoardCellType> oddFormat = new List<OperationsBoardCellType>() {
            OperationsBoardCellType.OP, OperationsBoardCellType.BLANK, OperationsBoardCellType.OP, OperationsBoardCellType.BLANK, OperationsBoardCellType.OP, OperationsBoardCellType.BLANK };
        List<OperationsBoardCellType> lastRowFormat = new List<OperationsBoardCellType>() {
            OperationsBoardCellType.ANSWER, OperationsBoardCellType.BLANK, OperationsBoardCellType.ANSWER, OperationsBoardCellType.BLANK, OperationsBoardCellType.ANSWER, OperationsBoardCellType.BLANK };
        for (int i = 0; i < board.GetLength(0) - 1; i++)
        {
            for (int p = 0; p < board.GetLength(1); p++)
            {
                List<OperationsBoardCellType> types = i % 2 == 0 ? evenFormat : oddFormat;
                board[i, p].SetCellType(types[p]);
            }
        }
        for (int i = 0; i < board.GetLength(1); i++)
        {
            board[board.GetLength(0) - 1, i].SetCellType(lastRowFormat[i]);
        }

        // Set all cells that have been defined as blank to be blank
        ActOnEachBoardCell(cell =>
        {
            if (cell.CellType == OperationsBoardCellType.BLANK)
            {
                cell.SetBlank();
            }
        });

        acceptedNums.Shuffle();
        PopulateBoard(populateBoardWith, acceptedOps, maxOpsDict);

        // Lock answer and operation cells
        ActOnEachBoardCell(cell =>
        {
            if (cell.CellType == OperationsBoardCellType.ANSWER || cell.CellType == OperationsBoardCellType.OP)
            {
                cell.Lock();
            }
        });

        yield return new WaitForSeconds(1);

        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLength(1); p++)
            {
                AudioManager._Instance.PlayFromSFXDict(onCellSpawned);

                OperationsBoardCell cell = board[i, p];

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

    private OperationsBoardCell FindEmptyCell()
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLength(1); p++)
            {
                // Debug.Log(i + ", " + p);
                OperationsBoardCell cell = board[i, p];
                if (cell.GetCorrectChar().Equals('0') && (cell.CellType == OperationsBoardCellType.NUM || cell.CellType == OperationsBoardCellType.OP))
                {
                    return cell;
                }
            }
        }
        return null;
    }

    private bool CheckForCellValidity(OperationsBoardCell cell)
    {
        return VerifyRow(cell) && VerifyColumn(cell);
    }

    private bool VerifyRow(OperationsBoardCell cell)
    {
        int cellRow = cell.Coordinates.x;
        // Debug.Log("Verifying Row of : " + cell);
        MathematicalEquation equation = new MathematicalEquation();
        for (int i = 0; i < board.GetLength(0); i++)
        {
            OperationsBoardCell checkCell = board[cellRow, i];
            switch (checkCell.CellType)
            {
                case OperationsBoardCellType.ANSWER:
                    if (equation.GetResult() < 0)
                    {
                        // Debug.Log("Answer: " + equation.GetResult() + ", Invalid Row");
                        return false;
                    }
                    else
                    {
                        // Debug.Log("Answer: " + equation.GetResult() + ", Valid Row");
                        checkCell.SetAnswer(equation.GetResult());
                        return true;
                    }
                case OperationsBoardCellType.BLANK:
                    // Debug.Log("Blank: Continue");
                    break;
                case OperationsBoardCellType.EMPTY:
                    // Debug.Log("Empty: Continue");
                    break;
                case OperationsBoardCellType.NUM:
                    if (checkCell.GetCorrectChar().Equals('0'))
                    {
                        // Debug.Log("Num: Unfilled, Valid Row");
                        return true;
                    }

                    int num = Utils.ConvertCharToInt(checkCell.GetCorrectChar());
                    equation.Apply(num);
                    // Debug.Log("Num: " + num + ", Applied to Equation");

                    if (equation.GetResult() < 0)
                    {
                        // Debug.Log("Num: " + equation.GetResult() + ", Invalid Row");
                        return false;
                    }

                    break;
                case OperationsBoardCellType.OP:
                    if (checkCell.GetCellOperation() == MathematicalOperation.NONE)
                    {
                        // Debug.Log("Op: Unfilled, Valid Row");
                        return true;
                    }

                    MathematicalOperation op = checkCell.GetCellOperation();
                    equation.Apply(op);
                    // Debug.Log("Op: " + op + ", Applied to Equation");

                    if (equation.GetResult() < 0)
                    {
                        // Debug.Log("Op: " + equation.GetResult() + ", Invalid Row");
                        return false;
                    }

                    break;
            }
        }
        return true;
    }

    private bool VerifyColumn(OperationsBoardCell cell)
    {
        int cellCol = cell.Coordinates.y;
        // Debug.Log("Verifying Column of : " + cell);
        MathematicalEquation equation = new MathematicalEquation();
        for (int i = 0; i < board.GetLength(1); i++)
        {
            OperationsBoardCell checkCell = board[i, cellCol];
            switch (checkCell.CellType)
            {
                case OperationsBoardCellType.ANSWER:
                    if (equation.GetResult() < 0)
                    {
                        // Debug.Log("Answer: " + equation.GetResult() + ", Invalid Row");
                        return false;
                    }
                    else
                    {
                        // Debug.Log("Answer: " + equation.GetResult() + ", Valid Row");
                        checkCell.SetAnswer(equation.GetResult());
                        return true;
                    }
                case OperationsBoardCellType.BLANK:
                    //  Debug.Log("Blank: Continue");
                    break;
                case OperationsBoardCellType.EMPTY:
                    // Debug.Log("Empty: Continue");
                    break;
                case OperationsBoardCellType.NUM:
                    if (checkCell.GetCorrectChar().Equals('0'))
                    {
                        // Debug.Log("Num: Unfilled, Valid Row");
                        return true;
                    }

                    int num = Utils.ConvertCharToInt(checkCell.GetCorrectChar());
                    equation.Apply(num);
                    // Debug.Log("Num: " + num + ", Applied to Equation");

                    if (equation.GetResult() < 0)
                    {
                        // Debug.Log("Num: " + equation.GetResult() + ", Invalid Row");
                        return false;
                    }

                    break;
                case OperationsBoardCellType.OP:
                    if (checkCell.GetCellOperation() == MathematicalOperation.NONE)
                    {
                        // Debug.Log("Op: Unfilled, Valid Row");
                        return true;
                    }

                    MathematicalOperation op = checkCell.GetCellOperation();
                    equation.Apply(op);
                    // Debug.Log("Op: " + op + ", Applied to Equation");

                    if (equation.GetResult() < 0)
                    {
                        // Debug.Log("Op: " + equation.GetResult() + ", Invalid Row");
                        return false;
                    }

                    break;
            }
        }
        return true;
    }

    private bool PopulateBoard(List<int> acceptedNums, List<MathematicalOperation> acceptedOperations, SerializableDictionary<MathematicalOperation, int> maxOperationsDict)
    {
        OperationsBoardCell cell = FindEmptyCell();
        // Debug.Log("Populating Cell: " + cell);

        // if there are no empty cells, the board is complete
        if (!cell) return true;

        switch (cell.CellType)
        {
            case OperationsBoardCellType.NUM:
                for (int i = 0; i < acceptedNums.Count; i++)
                {
                    int num = acceptedNums[i];
                    // Debug.Log("NUM: Attempting to Place: " + num + " - into: " + cell + ", " + num);

                    // Debug.Log("Attempting to Place: " + num + " - into: " + cell);

                    cell.SetCorrectChar(num.ToString()[0]);

                    if (CheckForCellValidity(cell))
                    {
                        acceptedNums.Remove(num);

                        // Debug.Log("1. Next: " + cell);
                        // PrintBoardState();
                        // Debug.Log("Placement Valid");
                        if (PopulateBoard(acceptedNums, acceptedOperations, maxOperationsDict))
                        {
                            // Debug.Log("3. Success: " + cell);
                            // Debug.Log("Board Population Successful");
                            return true;
                        }

                        acceptedNums.Add(num);

                        // Debug.Log("Board Population Unsuccessful; Future Placement Was Invalid");
                        cell.SetCorrectChar('0');

                        // PrintBoardState();
                    }
                }

                cell.SetCorrectChar('0');

                // Debug.Log("2. Back: " + cell);
                // Debug.Log("Board Population Unsuccessful; No Acceptable Nums");
                return false;
            case OperationsBoardCellType.OP:
                acceptedOperations.Shuffle();
                for (int i = 0; i < acceptedOperations.Count; i++)
                {
                    MathematicalOperation op = acceptedOperations[i];
                    // Debug.Log("OP: Attempting to Place: " + op + " - into: " + cell);

                    // Debug.Log("Attempting to Place: " + num + " - into: " + cell);

                    bool didRemoveOp = false;
                    if (maxOperationsDict.ContainsKey(op))
                    {
                        maxOperationsDict[op]--;
                        // Debug.Log("Contains Key: " + op + ", New Value: " + maxOperationsDict[op]);
                        if (maxOperationsDict[op] <= 0)
                        {
                            // Debug.Log("Removing Key: " + op);
                            maxOperationsDict.RemoveEntry(op);
                            acceptedOperations.Remove(op);
                            didRemoveOp = true;
                        }
                    }

                    cell.SetOperation(op);

                    if (CheckForCellValidity(cell))
                    {
                        // Debug.Log("2. Next: " + cell);
                        // PrintBoardState();
                        // Debug.Log("Placement Valid");
                        if (PopulateBoard(acceptedNums, acceptedOperations, maxOperationsDict))
                        {
                            // Debug.Log("4. Success: " + cell);
                            // Debug.Log("Board Population Successful");
                            return true;
                        }

                        // Debug.Log("Board Population Unsuccessful; Future Placement Was Invalid");
                        // PrintBoardState();
                    }
                    cell.Clear();

                    if (didRemoveOp)
                    {
                        // Debug.Log("Removed Op: " + op);
                        maxOperationsDict.Set(op, 1);
                        acceptedOperations.Add(op);
                    }

                    if (maxOperationsDict.ContainsKey(op))
                    {
                        maxOperationsDict[op]++;
                        // Debug.Log("Re-Increased Op: " + op + ", New Value: " + maxOperationsDict[op]);
                    }
                }
                return false;
        }
        return false;
    }

    public void RemoveCharFromInvalidLocations(char v)
    {
        RemoveCharFromBoard(v);
    }

    public OperationsBoardCell CheckIfBoardContainsChar(char v)
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLength(1); p++)
            {
                OperationsBoardCell cell = board[i, p];
                if (cell.CellType == OperationsBoardCellType.NUM && cell.GetInputtedChar().Equals(v))
                    return cell;
            }
        }
        return null;
    }

    private void RemoveCharFromBoard(char v)
    {
        ActOnEachBoardCell(cell =>
        {
            cell.TryRemovePencilChar(v);
            if (cell.GetInputtedChar().Equals(v))
            {
                cell.SetInputtedChar(' ');
            }
        });
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

    public void CheatBoard()
    {
        foreach (OperationsBoardCell cell in board)
        {
            if (cell.CellType == OperationsBoardCellType.NUM)
            {
                cell.SetInputtedChar(cell.GetCorrectChar());
            }
        }
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
        foreach (OperationsBoardCell cell in board)
        {
            if (cell.CellType == OperationsBoardCellType.NUM)
            {
                if (!cell.HasCorrectChar()) return false;
            }
        }
        return true;
    }
}
