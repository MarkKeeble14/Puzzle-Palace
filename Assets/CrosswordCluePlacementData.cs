using System.Collections.Generic;

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
