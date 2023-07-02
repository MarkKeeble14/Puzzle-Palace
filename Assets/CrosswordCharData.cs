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
