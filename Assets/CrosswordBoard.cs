using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public struct WordPlacementData
{
    public int Position;
    public char Character;

    public WordPlacementData(int position, char character)
    {
        Position = position;
        Character = character;
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
    private List<CrosswordBoardCell> placedIntoCells = new List<CrosswordBoardCell>();
    private Direction direction;

    public Alignment GetAlignment()
    {
        return direction == Direction.DOWN || direction == Direction.UP ? Alignment.VERTICAL : Alignment.HORIZONTAL;
    }

    public CrosswordClue GetClue()
    {
        return clue;
    }

    public List<CrosswordBoardCell> GetCrosswordBoardCells()
    {
        return placedIntoCells;
    }

    public void SetDirection(Direction d)
    {
        direction = d;
    }

    public void SetClue(CrosswordClue clue)
    {
        this.clue = clue;
    }

    public void AddCell(CrosswordBoardCell cell)
    {
        placedIntoCells.Add(cell);
    }

    public void RemoveCell(CrosswordBoardCell cell)
    {
        placedIntoCells.Remove(cell);
    }

    public override string ToString()
    {
        string cells = "";
        foreach (CrosswordBoardCell cell in placedIntoCells)
        {
            cells += cell + " ";
        }
        return base.ToString() + ": Clue: " + clue + ", Direction: " + direction + ", Num Cells: " + placedIntoCells.Count + " - " + cells;
    }
}

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
        int numEntries = 100000;
        int inc = 4;
        Dictionary<int, List<CrosswordClue>> charsPerAnswerDict = new Dictionary<int, List<CrosswordClue>>();
        for (int i = 0; i < numEntries * inc; i += inc)
        {
            // Debug.Log(new CrosswordClue(data[i + 3], data[i + 2]));
            CrosswordClue clue = new CrosswordClue(data[i + 3], data[i + 2]);

            int length = clue.GetAnswer().Length;
            if (charsPerAnswerDict.ContainsKey(length))
            {
                charsPerAnswerDict[length].Add(clue);
            }
            else
            {
                charsPerAnswerDict.Add(length, new List<CrosswordClue>() { clue });
            }
        }
        foreach (KeyValuePair<int, List<CrosswordClue>> kvp in charsPerAnswerDict)
        {
            kvp.Value.Shuffle();
        }

        List<CrosswordCluePlacementData> boardPlacements = new List<CrosswordCluePlacementData>();
        CreateGrid(charsPerAnswerDict, targetNumWords, minMaxWordSize, boardPlacements);

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

    private List<CrosswordCluePlacementData> GetCurrentBoardStatePlacementData()
    {
        List<CrosswordCluePlacementData> newData = new List<CrosswordCluePlacementData>();
        newData.AddRange(GetNewBoardPlacementDatas(Alignment.HORIZONTAL));
        newData.AddRange(GetNewBoardPlacementDatas(Alignment.VERTICAL));
        return newData;
    }

    private bool ValidateGrid()
    {
        List<CrosswordCluePlacementData> newData = GetCurrentBoardStatePlacementData();
        foreach (CrosswordCluePlacementData data in newData)
        {
            if (data.GetCrosswordBoardCells().Count == 2)
            {
                return false;
            }
        }
        return true;
    }

    private bool CreateGrid(Dictionary<int, List<CrosswordClue>> clues, int targetNumWords, Vector2Int minMaxWordLength, List<CrosswordCluePlacementData> boardPlacements)
    {
        int currentNumPlacements = GetCurrentBoardStatePlacementData().Count;
        // Debug.Log("Current Num Placements: " + currentNumPlacements + ", Target: " + targetNumWords);

        // Reached the targeted number of words
        if (currentNumPlacements >= targetNumWords)
        {
            List<CrosswordCluePlacementData> finalPlacementData = GetCurrentBoardStatePlacementData();

            Debug.Log("Grid Created");
            for (int i = 0; i < finalPlacementData.Count; i++)
            {
                Debug.Log(i + ", " + finalPlacementData[i]);
            }

            List<CrosswordCluePlacementData> newBoardPlacements = GetCurrentBoardStatePlacementData();
            if (PopulateBoard(clues, newBoardPlacements, 0))
            {
                Debug.Log("Full Success");
                return true;
            }

            Debug.Log("Created an Impossible Grid - Failed to Populate Board");

            return false;
        }

        if (boardPlacements.Count == 0)
        {
            CrosswordBoardCell cell = board[0, 0];
            List<Direction> directions = new List<Direction>() { Direction.DOWN, Direction.RIGHT };
            CrosswordCluePlacementData placementData = new CrosswordCluePlacementData();
            boardPlacements.Add(placementData);
            ReserveBoardSpaces(cell, RandomHelper.GetRandomFromList(directions), RandomHelper.RandomIntExclusive(minMaxWordLength), placementData);
            return CreateGrid(clues, targetNumWords, minMaxWordLength, boardPlacements);
        }
        else
        {
            boardPlacements.Shuffle();
            for (int i = 0; i < boardPlacements.Count; i++)
            {
                List<CrosswordBoardCell> possibleCells = boardPlacements[i].GetCrosswordBoardCells();
                for (int p = 0; p < possibleCells.Count; p++)
                {
                    CrosswordBoardCell cell = RandomHelper.GetRandomFromList(possibleCells);
                    List<Direction> directions = new List<Direction>() { Direction.DOWN, Direction.UP, Direction.LEFT, Direction.RIGHT };
                    directions.Shuffle();
                    CrosswordCluePlacementData placementData = new CrosswordCluePlacementData();
                    while (directions.Count > 0)
                    {
                        Direction d = directions[0];
                        if (ReserveBoardSpaces(cell, d, RandomHelper.RandomIntExclusive(minMaxWordLength), placementData))
                        {
                            boardPlacements.Add(placementData);
                            if (CreateGrid(clues, targetNumWords, minMaxWordLength, boardPlacements))
                            {
                                return true;
                            }
                            boardPlacements.Remove(placementData);
                        }
                        List<CrosswordBoardCell> changedCells = placementData.GetCrosswordBoardCells();
                        for (int q = 0; q < changedCells.Count; q++)
                        {
                            changedCells[q].RemoveReservedBy(placementData);
                        }
                        placementData.GetCrosswordBoardCells().Clear();
                        directions.RemoveAt(0);
                        boardPlacements.Remove(placementData);
                    }
                }
            }
        }
        Debug.Log("Failed to Create Board");

        for (int i = 0; i < boardPlacements.Count; i++)
        {
            Debug.Log(boardPlacements[i]);
        }

        return false;
    }

    private bool ReserveBoardSpaces(CrosswordBoardCell startCell, Direction d, int numCells, CrosswordCluePlacementData placementData)
    {
        // Debug.Log("Reserving " + numCells + " Cells, Direction: " + d + ", Start Cell: " + startCell);
        placementData.SetDirection(d);
        switch (d)
        {
            case Direction.RIGHT:
                for (int i = 0; i < numCells; i++)
                {
                    if (!ReserveBoardSpace(startCell, i, 0, placementData))
                    {
                        OnPlacementFailed(placementData);
                        return false;
                    }
                    else if (i == numCells - 1 && !ValidateGrid())
                    {
                        OnPlacementFailed(placementData);
                        return false;
                    }
                }
                return true;
            case Direction.LEFT:
                for (int i = 0; i < numCells; i++)
                {
                    if (!ReserveBoardSpace(startCell, -i, 0, placementData))
                    {
                        OnPlacementFailed(placementData);
                        return false;
                    }
                    else if (i == numCells - 1 && !ValidateGrid())
                    {
                        OnPlacementFailed(placementData);
                        return false;
                    }
                }
                return true;
            case Direction.UP:
                for (int i = 0; i < numCells; i++)
                {
                    if (!ReserveBoardSpace(startCell, 0, -i, placementData))
                    {
                        OnPlacementFailed(placementData);
                        return false;
                    }
                    else if (i == numCells - 1 && !ValidateGrid())
                    {
                        OnPlacementFailed(placementData);
                        return false;
                    }
                }
                return true;
            case Direction.DOWN:
                for (int i = 0; i < numCells; i++)
                {
                    if (!ReserveBoardSpace(startCell, 0, i, placementData))
                    {
                        OnPlacementFailed(placementData);
                        return false;
                    }
                    else if (i == numCells - 1 && !ValidateGrid())
                    {
                        OnPlacementFailed(placementData);
                        return false;
                    }
                }
                return true;
        }
        return false;
    }

    private bool ReserveBoardSpace(CrosswordBoardCell startCell, int colOffset, int rowOffset, CrosswordCluePlacementData placementData)
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
            // bool hasAdjacentParallels = HasAdjacentParallels(cell, placementData);
            // Debug.Log("Has Adjacent Parallels: " + hasAdjacentParallels);
            if (cellReservedBy.Count == 0)
            {
                // Go Ahead, add cell
                // Debug.Log("Cell Not Reserved Prior - Successfully Reserved");
                placementData.AddCell(cell);
                cell.SetReservedBy(placementData);
                return true;
            }
            else if (cellReservedBy.Count == 1)
            {
                if (cell.GetReservedBy()[0].GetAlignment() != placementData.GetAlignment())
                {
                    // Go Ahead, add cell
                    // Debug.Log("Cell Reserved Prior, Alignment Differs - Successfully Reserved");
                    placementData.AddCell(cell);
                    cell.SetReservedBy(placementData);
                    return true;
                }
            }
        }
        else
        {
            // Fail
            // Debug.Log("Cell Fully Reserved Prior - Failed to Reserve Cell: " + cell);
            cell.RemoveReservedBy(placementData);
            return false;

        }
        // Debug.Log("Failed to Reserve Cell: " + cell + ", No Case Defined");
        return false;
    }

    private void OnPlacementFailed(CrosswordCluePlacementData placementData)
    {
        while (placementData.GetCrosswordBoardCells().Count > 0)
        {
            CrosswordBoardCell cell = placementData.GetCrosswordBoardCells()[0];
            placementData.RemoveCell(cell);
            cell.Clear();
            cell.RemoveReservedBy(placementData);
        }
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
                if (cell.GetReservedBy().Count > 0)
                {
                    if (newPlacementData == null)
                    {
                        newPlacementData = new CrosswordCluePlacementData();
                        newPlacementData.SetDirection(directionalAlignment == Alignment.HORIZONTAL ? Direction.RIGHT : Direction.DOWN);
                    }
                    newPlacementData.AddCell(cell);
                }
                else
                {
                    if (newPlacementData != null)
                    {
                        if (newPlacementData.GetCrosswordBoardCells().Count > 1)
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
                if (newPlacementData.GetCrosswordBoardCells().Count > 1)
                {
                    // Debug.Log("Added New Placement Data: " + newPlacementData);
                    datas.Add(newPlacementData);
                }
                newPlacementData = null;
            }
        }
        return datas;
    }

    private bool WordMatchesData(string s, List<WordPlacementData> mustMatchData)
    {
        foreach (WordPlacementData data in mustMatchData)
        {
            if (s[data.Position] != data.Character)
            {
                return false;
            }
        }
        return true;
    }

    private bool PopulateBoard(Dictionary<int, List<CrosswordClue>> clues, List<CrosswordCluePlacementData> placedWords, int filledWords)
    {
        // Populate
        // Debug.Log("Populating: " + filledWords);

        if (filledWords == placedWords.Count)
        {
            Debug.Log("Board Populated");
            for (int i = 0; i < placedWords.Count; i++)
            {
                Debug.Log(i + ": " + placedWords[i]);
            }
            return true;
        }

        // Get the placement at passed in Index
        CrosswordCluePlacementData current = placedWords[filledWords];
        // Debug.Log("Filling In: " + current);

        // Keep a list of Crossword Clues tried so the algorithm knows if there are no possible ones
        List<CrosswordClue> attemptedClues = new List<CrosswordClue>();

        bool hasPlacedWord = false;
        while (!hasPlacedWord)
        {
            string s = "";
            List<CrosswordBoardCell> currentBoardCells = current.GetCrosswordBoardCells();
            List<WordPlacementData> requiredChars = new List<WordPlacementData>();
            for (int i = 0; i < currentBoardCells.Count; i++)
            {
                char currentChar = currentBoardCells[i].GetCorrectChar();
                if (!currentChar.Equals(CrosswordBoardCell.DefaultChar))
                    requiredChars.Add(new WordPlacementData(i, currentChar));
                s += currentChar;
            }
            Debug.Log("Currently in Placement" + current + ", " + s);

            // Get a clue that hasn't already been tried
            CrosswordClue clue = GetCrosswordClueWithCondition(clues[currentBoardCells.Count], clue =>
                !attemptedClues.Contains(clue) && WordMatchesData(clue.GetAnswer(), requiredChars));
            // Debug.Log("Attempting Clue: " + clue);

            // if the clue returned is null, then there are no matching clues
            if (clue == null)
            {
                Debug.Log("No Matching Clues at filledWords = " + filledWords);
                return false;
            }

            // Try placing the clue
            hasPlacedWord = TryPlaceWord(clue, current);
            PrintBoardState();
            // Debug.Log("Result: " + hasPlacedWord);

            // if the clue fits properly, remove that clue from the total list of clues so that we do not see it again
            // increment filled words, move onto the next clue
            // if the clue does not fit properly, add that clue to the list of tried clues and continue looping until either we find a working clue or there are no possible clues remaining
            if (hasPlacedWord)
            {
                // Debug.Log("Success: Removing Clue: " + clue + " From Future Placement Options");
                clues[current.GetCrosswordBoardCells().Count].Remove(clue);
                if (PopulateBoard(clues, placedWords, ++filledWords))
                {
                    Debug.Log("Success at filledWords = " + filledWords);
                    return true;
                }

                filledWords -= 1;

                // Re-add word to list of possible words
                clues[current.GetCrosswordBoardCells().Count].Add(clue);

                // Reset
                List<CrosswordBoardCell> changedCells = current.GetCrosswordBoardCells();
                for (int i = 0; i < changedCells.Count; i++)
                {
                    changedCells[i].SetCorrectChar(CrosswordBoardCell.DefaultChar);
                    changedCells[i].SetInputtedChar(' ');
                }
                current.SetClue(null);

                hasPlacedWord = false;
            }
            else
            {
                // Debug.Log("Failure: Removing Clue: " + clue + " From Current Options");
                attemptedClues.Add(clue);
            }
        }

        Debug.Log("Failed to Populate Board at filledWords = " + filledWords);
        return false;
    }

    private bool TryPlaceWord(CrosswordClue clue, CrosswordCluePlacementData placeInto)
    {
        List<CrosswordBoardCell> cells = placeInto.GetCrosswordBoardCells();
        placeInto.SetClue(clue);
        string answer = clue.GetAnswer();
        List<CrosswordBoardCell> changedCells = new List<CrosswordBoardCell>();
        for (int i = 0; i < cells.Count; i++)
        {
            CrosswordBoardCell cell = cells[i];
            if (cell.GetCorrectChar().Equals(CrosswordBoardCell.DefaultChar))
            {
                // Success Case, Correct Char Not Already Within Cell but can be placed there
                changedCells.Add(cell);
                cell.SetCorrectChar(answer[i]);

                cell.SetInputtedChar(cells[i].GetCorrectChar());
            }
            else if (cell.GetCorrectChar().Equals(answer[i]))
            {
                // Success Case, Correct Char Already Within Cell
                // No need to change Cell
            }
            else
            {
                // Fail Case, Word cannot be placed into this board place
                // Reset Previously altered Cells & Remove Data from PlacementData
                for (int p = 0; p < changedCells.Count; p++)
                {
                    changedCells[p].Clear();
                }
                placeInto.SetClue(null);
                return false;
            }
        }
        return true;
    }

    private CrosswordClue GetCrosswordClueWithCondition(List<CrosswordClue> possibleClues, Func<CrosswordClue, bool> condition)
    {
        List<CrosswordClue> disallowedClues = new List<CrosswordClue>();
        for (int i = 0; i < possibleClues.Count; i++)
        {
            CrosswordClue tryingClue = possibleClues[i];
            if (disallowedClues.Contains(tryingClue)) continue;
            if (condition(tryingClue)) return tryingClue;
            disallowedClues.Add(tryingClue);
        }
        return null;
    }

    private List<CrosswordBoardCell> GetSurroundingCells(CrosswordBoardCell centerCell)
    {
        List<CrosswordBoardCell> res = new List<CrosswordBoardCell>();
        Vector2Int center = centerCell.Coordinates;
        for (int i = -1; i <= 1; i++)
        {
            for (int p = -1; p <= 1; p++)
            {
                if (center.x + i > board.GetLength(0) - 1 || center.x + i < 0
                    || center.y + p > board.GetLength(1) - 1 || center.y + p < 0) continue;
                res.Add(board[center.x + i, center.y + p]);
            }
        }
        return res;
    }

    private void PrintBoardState()
    {
        string s = "";
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
            if (!cell.HasCorrectChar()) return false;
        }
        return true;
    }
}