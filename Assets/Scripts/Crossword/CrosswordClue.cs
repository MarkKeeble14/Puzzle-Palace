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
