using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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

    public IEnumerator Generate(Action<CrosswordBoardCell> onPressCellAction, Dictionary<int, List<CrosswordClue>> clueDict, List<CrosswordClue> clues, List<string> possibleAnswers,
        Vector2Int boardSize, Vector2Int minMaxWordSize, int targetNumWords, int minNumWords, int maxAlottedAttemptsPerWord, int maxRetryAttempts)
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

        List<CrosswordCluePlacementData> boardPlacements = new List<CrosswordCluePlacementData>();

        // Populate Board
        CreateBoard(clues, possibleAnswers, targetNumWords, minNumWords, maxAlottedAttemptsPerWord, maxRetryAttempts, minMaxWordSize, boardPlacements);

        // PrintBoardState();
        List<CrosswordCluePlacementData> finalBoardPlacements = GetCurrentBoardStatePlacementData();

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
        }

        // Print Final Result of Generation
        for (int i = 0; i < boardPlacements.Count; i++)
        {
            Debug.Log("Completed Board Placement: " + boardPlacements[i]);
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

    private bool CreateBoard(List<CrosswordClue> clues, List<string> possibleAnswers, int targetNumWords, int minNumWords,
        int allottedSearchAmount, int maxRetryAttempts, Vector2Int minMaxWordLength, List<CrosswordCluePlacementData> boardPlacements)
    {
        // PrintBoardState("At Start of CreateBoard");

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
            int retryAttempts = 0;
            CrosswordBoardCell cell = board[0, 0];
            List<Direction> directions = new List<Direction>() { Direction.DOWN, Direction.RIGHT };
            CrosswordCluePlacementData placementData = new CrosswordCluePlacementData();
            List<CrosswordClue> attemptedClues = new List<CrosswordClue>();

            while (true)
            {
                // if tried to generate several times and no successes, reduce the number of target words to make generation more likely
                // doing this to prevent full hangups
                if (targetNumWords > minNumWords && retryAttempts > maxRetryAttempts)
                {
                    if (targetNumWords - 1 >= minNumWords)
                    {
                        Debug.Log("Hit Max Attempts, Reducing Complexity - TargetNumWords now at: " + --targetNumWords);

                    }
                    else
                    {
                        Debug.Log("Hit Max Attempts, Disallowed to Reduce Complexity - TargetNumWords remains at: " + targetNumWords);
                    }
                }

                // Get a new clue
                CrosswordClue clue = RandomHelper.GetRandomFromList(clues);
                attemptedClues.Add(clue);
                clues.Remove(clue);

                // Try to place that word in the top left cell, either rightwards or downwards
                if (TryPlaceWord(cell, RandomHelper.GetRandomFromList(directions), clue, placementData))
                {
                    // PrintBoardState("After TryPlaceWord, No Board Placement");
                    foreach (CrosswordClue attemptedClue in attemptedClues)
                    {
                        if (attemptedClue != clue)
                            clues.Add(attemptedClue);
                    }

                    // If able to do so (which you always should be able to), try to generate the rest of the board
                    // Add the word to the tracked placements
                    // Remove the clue from the available clues
                    boardPlacements.Add(placementData);

                    if (CreateBoard(clues, possibleAnswers, targetNumWords, minNumWords, allottedSearchAmount, maxRetryAttempts, minMaxWordLength, boardPlacements))
                    {
                        return true;
                    }

                    retryAttempts++;

                    // Failed to generate the rest of the board
                    // Remove the placed letters, remove the word from the tracked placements, and re-add it to the available clues
                    OnPlacementFailed(placementData);
                    boardPlacements.Remove(placementData);

                    // Try a new word
                }
            }
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
                                // Debug.Log("Successfully Placed Clue: " + clue);
                                // PrintBoardState("After TryPlaceWord, More Than 0 Board Placement");
                                boardPlacements.Add(placementData);
                                clues.Remove(clue);

                                // Debug.Log("Successfully Placed Word: " + clue);
                                // PrintBoardState();

                                if (ValidateGrid(possibleAnswers))
                                {
                                    // Debug.Log("Valid Grid");

                                    if (CreateBoard(clues, possibleAnswers, targetNumWords, minNumWords, allottedSearchAmount, maxRetryAttempts, minMaxWordLength, boardPlacements))
                                    {
                                        return true;
                                    }
                                }

                                // Debug.Log("Invalid Grid or Failed to Create Board in the Future");

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
        // Debug.Log("Trying to Place Char: " + c + ", Cell: " + cell + ", Cell Char: " + cell.GetCorrectChar());
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

    private void PrintBoardState(string addedDescriptor = "")
    {
        string s = "Printing Board State" + (addedDescriptor.Length > 0 ? ": " : "") + addedDescriptor + "\n";
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