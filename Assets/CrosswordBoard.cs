using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CrosswordCharData
{
    public char Character;
    public CrosswordBoardCell Cell;

    public CrosswordCharData(char character, CrosswordBoardCell cell)
    {
        Character = character;
        Cell = cell;
    }
}

public enum Direction
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}

public enum Alignment
{
    VERTICAL,
    HORIZONTAL
}

[System.Serializable]
public class CrosswordCluePlacementData
{
    private CrosswordClue clue;
    private List<CrosswordBoardCell> incorperatedCells = new List<CrosswordBoardCell>();
    private List<CrosswordBoardCell> affectedCells = new List<CrosswordBoardCell>();
    private Direction direction;

    public Alignment GetAlignment()
    {
        return direction == Direction.DOWN || direction == Direction.UP ? Alignment.VERTICAL : Alignment.HORIZONTAL;
    }

    public CrosswordClue GetClue()
    {
        return clue;
    }

    public List<CrosswordBoardCell> GetAffectedCells()
    {
        return affectedCells;
    }

    public void SetDirection(Direction d)
    {
        direction = d;
    }

    public void SetClue(CrosswordClue clue)
    {
        this.clue = clue;
    }

    public void AddIncorperatedCell(CrosswordBoardCell cell)
    {
        // Debug.Log(this + ", Adding Incorperated Cell: " + cell);
        incorperatedCells.Add(cell);
    }

    public void RemoveIncorperatedCell(CrosswordBoardCell cell)
    {
        // Debug.Log(this + ", Removing Incorperated Cell: " + cell);
        incorperatedCells.Remove(cell);
    }

    public void AddAffectedCell(CrosswordBoardCell cell)
    {
        affectedCells.Add(cell);
    }

    public void RemoveAffectedCell(CrosswordBoardCell cell)
    {
        affectedCells.Remove(cell);
    }

    public override string ToString()
    {
        /*
        string cells = "";
        foreach (CrosswordBoardCell cell in placedIntoCells)
        {
            cells += cell + " ";
        }
        */
        // return base.ToString() + ": Clue: " + clue + ", Direction: " + direction + ", Num Cells: " + placedIntoCells.Count + " - " + cells;
        return base.ToString() + ": Clue: " + clue + ", Makeup String: " + GetMakeupString();
    }

    public bool SharesAnyChar(CrosswordClue checkingClue)
    {
        string checkAnswer = checkingClue.GetAnswer();
        for (int i = 0; i < checkAnswer.Length; i++)
        {
            if (clue.GetAnswer().Contains(checkAnswer[i]))
                return true;
        }
        return false;
    }

    public List<CrosswordCharData> GetSharedChars(CrosswordClue nextClue)
    {
        string checkAnswer = nextClue.GetAnswer();

        List<CrosswordCharData> result = new List<CrosswordCharData>();

        // for each char in the passed in clue
        for (int i = 0; i < checkAnswer.Length; i++)
        {
            // loop through each cell in clue and see if it's char matches the char from the clue
            for (int q = 0; q < affectedCells.Count; q++)
            {
                if (affectedCells[q].GetCorrectChar().Equals(checkAnswer[i]))
                    result.Add(new CrosswordCharData(checkAnswer[i], affectedCells[q]));
            }
        }
        return result;
    }

    public string GetMakeupString()
    {
        string s = "";
        foreach (CrosswordBoardCell cell in incorperatedCells)
        {
            s += cell.GetCorrectChar();
        }
        return s;
    }

    public List<CrosswordBoardCell> GetIncorperatedCells()
    {
        return incorperatedCells;
    }
}

[System.Serializable]
public class CrosswordClue
{
    private string clue;
    private string answer;

    public CrosswordClue(string clue, string answer)
    {
        this.clue = clue;
        this.answer = answer;
    }

    public override string ToString()
    {
        return "Clue: " + clue + " - Answer: " + answer;
    }

    public string GetClue()
    {
        return clue;
    }

    public string GetAnswer()
    {
        return answer;
    }
}

public class CrosswordBoard : MonoBehaviour
{
    [Header("Data")]
    private CrosswordBoardCell[,] board;

    [Header("Visual & References")]
    [SerializeField] private CrosswordBoardCell boardCellPrefab;
    [SerializeField] private Transform parentCellSpawnsTo;
    [SerializeField] private float delayBetweenCellSpawns = 0.0f;
    [SerializeField] private float pitchIncreasePerCell;
    [SerializeField] private RectTransform mainTransform;
    [SerializeField] private CanvasGroup mainCanvasGroup;
    [SerializeField] private GridLayoutGroup glGroup;
    [SerializeField] private float changeAlphaRate = 5.0f;
    [SerializeField] private float changeScaleRate = 5.0f;

    [Header("Audio")]
    [SerializeField] private string onCellSpawned = "tttBoard_onCellSpawned";
    [SerializeField] private string onFinishedSpawningCells = "tttBoard_onFinishedSpawningCells";

    private bool interactable;

    public static List<char> _CharList = new List<char> { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

    public void SetInteractable(bool v)
    {
        interactable = v;
        foreach (CrosswordBoardCell cell in board)
        {
            cell.SetInteractable(interactable);
        }
    }

    public void ActOnEachBoardCell(Action<CrosswordBoardCell> func)
    {
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLength(1); p++)
            {
                func(board[i, p]);
            }
        }
    }

    public IEnumerator ActOnEachBoardCellWithDelay(Action<CrosswordBoardCell> func, float delay, bool reverseOrder)
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

    public void UnshowCellsWithChar()
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

    public void UnshowCells()
    {
        ActOnEachBoardCell(cell =>
        {
            cell.SetSymbolAlpha(1);
            cell.SetAllPencilledCharAlpha(0);
        });
    }

    public void ShowCells(CrosswordCluePlacementData toShow)
    {
        List<CrosswordBoardCell> showCells = toShow.GetIncorperatedCells();
        for (int i = 0; i < showCells.Count; i++)
        {
            CrosswordBoardCell cell = showCells[i];
            cell.SetSymbolAlpha(.75f);
        }
    }

    public IEnumerator Generate(Action<CrosswordBoardCell> onPressCellAction, string clueFilePath, Vector2Int boardSize, Vector2Int minMaxWordSize, int targetNumWords)
    {
        board = new CrosswordBoardCell[boardSize.x, boardSize.y];
        glGroup.constraintCount = boardSize.y;
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLength(1); p++)
            {
                // Debug.Log(i + ", " + p);
                CrosswordBoardCell cell = Instantiate(boardCellPrefab, parentCellSpawnsTo);
                cell.SetInteractable(false);
                cell.AddOnPressedAction(onPressCellAction);
                cell.Coordinates = new Vector2Int(i, p);
                cell.SetCorrectChar(CrosswordBoardCell.DefaultChar);
                cell.SetInputtedChar(' ');
                board[i, p] = cell;
                cell.name += "<" + i + ", " + p + ">";
            }
        }

        // Parse Data
        string st = File.ReadAllText(clueFilePath);
        string[] data = st.Split(new char[] { '\t', '\n' });

        // Populate Clue Dict with Lists
        Dictionary<int, List<CrosswordClue>> clueDict = new Dictionary<int, List<CrosswordClue>>();
        for (int i = minMaxWordSize.x; i <= minMaxWordSize.y; i++)
        {
            clueDict.Add(i, new List<CrosswordClue>());
        }

        // Populate Lists
        List<string> possibleAnswers = new List<string>();
        int numEntries = 2500;
        int inc = 16;
        int start = MathHelper.RoundToNearestGivenInt(RandomHelper.RandomIntExclusive(0, data.Length - (numEntries * inc)), 4);
        int cap = start + (numEntries * inc);
        // Debug.Log("Total: " + data.Length + ", NumEntries: " + numEntries + ", inc: " + inc + ", Start: " + start);
        for (int i = start; i < cap; i += inc)
        {
            string answer = data[i + 2];
            // Debug.Log(answer);
            if (answer.Length < minMaxWordSize.x || answer.Length > minMaxWordSize.y)
            {
                continue;
            }
            // Debug.Log(new CrosswordClue(data[i + 3], data[i + 2]));
            possibleAnswers.Add(answer);
            clueDict[answer.Length].Add(new CrosswordClue(data[i + 3], answer));
            // Debug.Log("Added Answer: " + answer);
            // in case the increment causes indexer to rise above the cap before filling in all the entries
            if (i + inc > cap && possibleAnswers.Count < numEntries)
            {
                i = 4;
            }
        }
        // Debug.Log("Done Parsing Data");

        // Shuffle Lists
        List<CrosswordClue> clues = new List<CrosswordClue>();
        foreach (KeyValuePair<int, List<CrosswordClue>> kvp in clueDict)
        {
            kvp.Value.Shuffle();
            clues.AddRange(kvp.Value);
        }

        // Sort Clue List
        CrosswordClue[] arr = clues.ToArray();
        Array.Sort(arr, (y, x) => x.GetAnswer().Length.CompareTo(y.GetAnswer().Length));
        clues = arr.ToList();
        // Debug.Log(clues.Count + ", " + clues[0] + ", " + clues[clues.Count - 1]);

        List<CrosswordCluePlacementData> boardPlacements = new List<CrosswordCluePlacementData>();
        bool boardGenerated = false;
        while (!boardGenerated)
        {
            // Populate Board
            boardGenerated = CreateBoard(clues, possibleAnswers, targetNumWords, 50, minMaxWordSize, boardPlacements);

            if (!boardGenerated)
            {
                Debug.Log("Failed to Generate, Re-Trying");
                ActOnEachBoardCell(cell =>
                {
                    cell.Clear();
                    cell.ResetReservedBy();
                });
                boardPlacements.Clear();
            }
        }
        // PrintBoardState();
        List<CrosswordCluePlacementData> finalBoardPlacements = GetCurrentBoardStatePlacementData();

        /*
        Debug.Log("Final Board Placements");
        for (int i = 0; i < finalBoardPlacements.Count; i++)
        {
            Debug.Log("Final Board Placement: " + finalBoardPlacements[i]);
        }
        Debug.Log("Calculated Board Placements");
        for (int i = 0; i < boardPlacements.Count; i++)
        {
            Debug.Log("Calculated Board Placement: " + boardPlacements[i]);
        }
        Debug.Log("Final Board Placements: " + finalBoardPlacements.Count + ", Calculated Board Placements: " + boardPlacements.Count);
        */

        // Determine the final board state if the number of placed words does not match the number of words on the board
        if (boardPlacements.Count != finalBoardPlacements.Count)
        {
            // PrintBoardState();
            ActOnEachBoardCell(cell =>
            {
                cell.ResetReservedBy();
            });
            boardPlacements = GetCurrentBoardStatePlacementData();

            // Attach Clue to Placements
            foreach (CrosswordCluePlacementData placement in boardPlacements)
            {
                string answer = placement.GetMakeupString();
                // Debug.Log("Missing Word: " + placement + ", Looking for: " + answer);
                List<CrosswordClue> searchThrough = clueDict[placement.GetMakeupString().Length];
                for (int i = 0; i < searchThrough.Count; i++)
                {
                    CrosswordClue cur = searchThrough[i];
                    // Debug.Log("Searching: " + cur);
                    if (cur.GetAnswer() == answer)
                    {
                        // Debug.Log("Found Clue: " + cur);
                        placement.SetClue(cur);
                        List<CrosswordBoardCell> includedCells = placement.GetIncorperatedCells();
                        // Debug.Log("CellCount: " + includedCells.Count);
                        foreach (CrosswordBoardCell cell in includedCells)
                        {
                            // Debug.Log("Set Reserved By for: " + cell);
                            cell.SetReservedBy(placement);
                        }
                        break;
                    }
                }
            }

            for (int i = 0; i < boardPlacements.Count; i++)
            {
                Debug.Log("Completed Board Placement: " + boardPlacements[i]);
            }
        }

        ActOnEachBoardCell(cell =>
        {
            if (cell.GetReservedBy().Count == 0)
            {
                cell.SetBlank();
            }
        });

        yield return new WaitForSeconds(1);

        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLength(1); p++)
            {
                AudioManager._Instance.PlayFromSFXDict(onCellSpawned);

                CrosswordBoardCell cell = board[i, p];

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

    private List<CrosswordCluePlacementData> GetNewBoardPlacementDatas(Alignment directionalAlignment)
    {
        List<CrosswordCluePlacementData> datas = new List<CrosswordCluePlacementData>();
        CrosswordCluePlacementData newPlacementData = null;
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLength(1); p++)
            {
                CrosswordBoardCell cell = board[directionalAlignment == Alignment.HORIZONTAL ? i : p, directionalAlignment == Alignment.HORIZONTAL ? p : i];
                if (!cell.GetCorrectChar().Equals(CrosswordBoardCell.DefaultChar))
                {
                    if (newPlacementData == null)
                    {
                        newPlacementData = new CrosswordCluePlacementData();
                        newPlacementData.SetDirection(directionalAlignment == Alignment.HORIZONTAL ? Direction.RIGHT : Direction.DOWN);
                    }
                    newPlacementData.AddAffectedCell(cell);
                    newPlacementData.AddIncorperatedCell(cell);
                }
                else
                {
                    if (newPlacementData != null)
                    {
                        if (newPlacementData.GetAffectedCells().Count > 1)
                        {
                            // Debug.Log("Added New Placement Data: " + newPlacementData);
                            datas.Add(newPlacementData);
                        }
                        newPlacementData = null;
                    }
                }
            }
            if (newPlacementData != null)
            {
                if (newPlacementData.GetAffectedCells().Count > 1)
                {
                    // Debug.Log("Added New Placement Data: " + newPlacementData);
                    datas.Add(newPlacementData);
                }
                newPlacementData = null;
            }
        }
        return datas;
    }

    private List<CrosswordCluePlacementData> GetCurrentBoardStatePlacementData()
    {
        List<CrosswordCluePlacementData> newData = new List<CrosswordCluePlacementData>();
        newData.AddRange(GetNewBoardPlacementDatas(Alignment.HORIZONTAL));
        newData.AddRange(GetNewBoardPlacementDatas(Alignment.VERTICAL));
        return newData;
    }

    private bool ValidateGrid(List<string> possibleAnswers)
    {
        // PrintBoardState();
        List<CrosswordCluePlacementData> newData = GetCurrentBoardStatePlacementData();
        foreach (CrosswordCluePlacementData data in newData)
        {
            // Debug.Log("Checking: " + data);
            if (!possibleAnswers.Contains(data.GetMakeupString()))
            {
                // Debug.Log(data.GetMakeupString() + ", Not Found Within Possible Answers");
                return false;
            }
            // Debug.Log(data.GetMakeupString() + ", Found Within Possible Answers");
        }
        // Debug.Log("Grid Valid");
        return true;
    }

    private bool CreateBoard(List<CrosswordClue> clues, List<string> possibleAnswers, int targetNumWords, int allottedSearchAmount, Vector2Int minMaxWordLength, List<CrosswordCluePlacementData> boardPlacements)
    {
        // Debug.Log("Create Board Called: NumWords = " + boardPlacements.Count);
        // PrintBoardState();
        if (clues.Count == 0)
        {
            Debug.Log("Ran out of Clues; Board Should Likely be Populated");
            return true;
        }

        if (boardPlacements.Count >= targetNumWords)
        {
            Debug.Log("Placed At Least the Target Amount out of Clues; Board Populated");
            return true;
        }

        if (boardPlacements.Count == 0)
        {
            CrosswordBoardCell cell = board[0, 0];
            List<Direction> directions = new List<Direction>() { Direction.DOWN, Direction.RIGHT };
            CrosswordCluePlacementData placementData = new CrosswordCluePlacementData();
            boardPlacements.Add(placementData);
            CrosswordClue used = RandomHelper.GetRandomFromList(clues);
            // Debug.Log("Clue: " + used);
            clues.Remove(used);
            TryPlaceWord(cell, RandomHelper.GetRandomFromList(directions), used, placementData);
            return CreateBoard(clues, possibleAnswers, targetNumWords, allottedSearchAmount, minMaxWordLength, boardPlacements);
        }
        else
        {
            int cap = clues.Count > allottedSearchAmount ? allottedSearchAmount : clues.Count;
            for (int i = 0; i < cap; i++)
            {
                CrosswordClue clue = RandomHelper.GetRandomFromList(clues);
                // Debug.Log("Clue: " + clue);

                // Search through the board for matching letters
                for (int p = 0; p < boardPlacements.Count; p++)
                {
                    // Debug.Log("Checking Board Placement: " + boardPlacements[p]);
                    List<CrosswordCharData> checkCharDataResult = boardPlacements[p].GetSharedChars(clue);
                    if (checkCharDataResult.Count > 0)
                    {
                        // Common letter between words
                        for (int q = 0; q < checkCharDataResult.Count; q++)
                        {
                            // Debug.Log("Shares Letter: " + checkCharDataResult[q].Character + " in Cell: " + checkCharDataResult[q].Cell);

                            // Try placing current clue based on this common letter - That would mean placing the current clue perpindicular to it somehow

                            // if successful, remove this clue from the list, move on and repeat this step
                            CrosswordCluePlacementData placementData = new CrosswordCluePlacementData();
                            // PrintBoardState();
                            if (TryPlaceWord(checkCharDataResult[q].Cell, clue, placementData, boardPlacements[p]))
                            {
                                boardPlacements.Add(placementData);
                                clues.Remove(clue);

                                // Debug.Log("Successfully Placed Word: " + clue);
                                // PrintBoardState();

                                if (ValidateGrid(possibleAnswers))
                                {
                                    // Debug.Log("Down, Valid Grid");

                                    if (CreateBoard(clues, possibleAnswers, targetNumWords, allottedSearchAmount, minMaxWordLength, boardPlacements))
                                    {
                                        return true;
                                    }
                                }

                                // Debug.Log("Up, Invalid Grid At NumWords = " + boardPlacements.Count);
                                OnPlacementFailed(placementData);
                                boardPlacements.Remove(placementData);
                                clues.Add(clue);
                            }
                        }
                    }
                    else
                    {
                        // if unsuccessful, continue along making sure to reset all changed cells
                        // Debug.Log("No Shared Chars");
                    }
                }
                // Looped through all placed words; no characters shared with the currently selected word, continue
            }
        }
        // Looped through all words and did not trigger a return condition from recursive call, inadequate board
        // Debug.Log("Did Not Reach Base Case");
        return false;
    }

    private bool TryPlaceWord(CrosswordBoardCell matchingCharCell, CrosswordClue newClue, CrosswordCluePlacementData newPlacementData, CrosswordCluePlacementData matchingPlacementData)
    {
        // Find the cell to start placing the new clue from
        CrosswordBoardCell cell = matchingCharCell;
        int numCellOffset = newClue.GetAnswer().IndexOf(matchingCharCell.GetCorrectChar());
        Direction d = matchingPlacementData.GetAlignment() == Alignment.VERTICAL ? Direction.RIGHT : Direction.DOWN;
        for (int i = 0; i < numCellOffset; i++)
        {
            switch (d)
            {
                case Direction.RIGHT:
                    if (cell.Coordinates.y - 1 < 0) return false;
                    cell = board[cell.Coordinates.x, cell.Coordinates.y - 1];
                    break;
                case Direction.DOWN:
                    if (cell.Coordinates.x - 1 < 0) return false;
                    cell = board[cell.Coordinates.x - 1, cell.Coordinates.y];
                    break;
            }
        }

        // We know our starting cell by now
        // Call other TryPlaceWord Accordingly
        return TryPlaceWord(cell, d, newClue, newPlacementData);
    }

    private bool TryPlaceWord(CrosswordBoardCell startCell, Direction d, CrosswordClue clue, CrosswordCluePlacementData placementData)
    {
        // Debug.Log("Reserving " + numCells + " Cells, Direction: " + d + ", Start Cell: " + startCell);
        placementData.SetDirection(d);
        placementData.SetClue(clue);
        string answer = clue.GetAnswer();

        switch (d)
        {
            case Direction.RIGHT:
                for (int i = 0; i < answer.Length; i++)
                {
                    if (!PlaceChar(answer[i], startCell, i, 0, placementData))
                    {
                        OnPlacementFailed(placementData);
                        return false;
                    }
                }
                return true;
            case Direction.LEFT:
                for (int i = 0; i < answer.Length; i++)
                {
                    if (!PlaceChar(answer[i], startCell, -i, 0, placementData))
                    {
                        OnPlacementFailed(placementData);
                        return false;
                    }
                }
                return true;
            case Direction.UP:
                for (int i = 0; i < answer.Length; i++)
                {
                    if (!PlaceChar(answer[i], startCell, 0, -i, placementData))
                    {
                        OnPlacementFailed(placementData);
                        return false;
                    }
                }
                return true;
            case Direction.DOWN:
                for (int i = 0; i < answer.Length; i++)
                {
                    if (!PlaceChar(answer[i], startCell, 0, i, placementData))
                    {
                        OnPlacementFailed(placementData);
                        return false;
                    }
                }
                return true;
        }
        return false;
    }

    private bool PlaceChar(char c, CrosswordBoardCell startCell, int colOffset, int rowOffset, CrosswordCluePlacementData placementData)
    {
        // Debug.Log("Attempting to Reserve Space: Coordinates: <" + (startCell.Coordinates.x + rowOffset) + ", " + (startCell.Coordinates.y + colOffset) + ">");
        // Guard against out of bounds
        if (startCell.Coordinates.x + rowOffset > board.GetLength(0) - 1
            || startCell.Coordinates.x + rowOffset < 0)
        {
            // Debug.Log("Fail: OOB on X");
            return false;
        }
        if (startCell.Coordinates.y + colOffset > board.GetLength(1) - 1
            || startCell.Coordinates.y + colOffset < 0)
        {
            // Debug.Log("Fail: OOB on Y");
            return false;
        }

        // try to reserve cell
        CrosswordBoardCell cell = board[startCell.Coordinates.x + rowOffset, startCell.Coordinates.y + colOffset];
        if (cell.CanBeReserved())
        {
            List<CrosswordCluePlacementData> cellReservedBy = cell.GetReservedBy();

            if (cellReservedBy.Count == 0)
            {
                // Go Ahead, add cell
                // Debug.Log("Cell Not Reserved Prior - Successfully Reserved");
                placementData.AddAffectedCell(cell);
                placementData.AddIncorperatedCell(cell);

                cell.SetReservedBy(placementData);

                cell.SetCorrectChar(c);

                return true;
            }
            else if (cellReservedBy.Count == 1)
            {
                if (cell.GetCorrectChar().Equals(c) &&
                    cell.GetReservedBy()[0].GetAlignment() != placementData.GetAlignment())
                {
                    // Go Ahead, add cell
                    // Debug.Log("Cell Reserved Prior, Same Char, Alignment Differs - Successfully Reserved");
                    cell.SetReservedBy(placementData);
                    placementData.AddIncorperatedCell(cell);

                    return true;
                }
            }
        }
        else
        {
            // Fail
            // Debug.Log("Cell Fully Reserved Prior - Failed to Reserve Cell: " + cell);
            return false;

        }
        // Debug.Log("Failed to Reserve Cell: " + cell + ", No Case Defined");
        return false;
    }

    private void OnPlacementFailed(CrosswordCluePlacementData placementData)
    {
        // Debug.Log("OnPlacementFailed: " + placementData);

        List<CrosswordBoardCell> affectedCells = placementData.GetAffectedCells();
        List<CrosswordBoardCell> incorperatedCells = placementData.GetIncorperatedCells();

        while (affectedCells.Count > 0)
        {
            CrosswordBoardCell cell = affectedCells[0];
            placementData.RemoveAffectedCell(cell);
            cell.Clear();
            placementData.RemoveIncorperatedCell(cell);
            cell.RemoveReservedBy(placementData);
        }

        while (incorperatedCells.Count > 0)
        {
            CrosswordBoardCell cell = incorperatedCells[0];
            placementData.RemoveIncorperatedCell(cell);
            cell.RemoveReservedBy(placementData);
        }

        placementData.SetClue(null);
    }

    private void PrintBoardState()
    {
        string s = "Printing Board State\n";
        for (int i = 0; i < board.GetLength(0); i++)
        {
            for (int p = 0; p < board.GetLength(1); p++)
            {
                if (board[i, p].GetReservedBy().Count == 0)
                {
                    s += "~";
                }
                else if (board[i, p].GetCorrectChar().Equals(CrosswordBoardCell.DefaultChar))
                {
                    s += "?";
                }
                else
                {
                    s += board[i, p].GetCorrectChar();
                }
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
        foreach (CrosswordBoardCell cell in board)
        {
            cell.SetInputtedChar(cell.GetCorrectChar());
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
        foreach (CrosswordBoardCell cell in board)
        {
            if (cell.GetReservedBy().Count == 0) continue;
            if (!cell.HasCorrectChar()) return false;
        }
        return true;
    }
}