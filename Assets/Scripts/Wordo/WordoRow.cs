using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WordoRow : MonoBehaviour
{
    [SerializeField] private CanvasGroup mainCV;
    [SerializeField] private float alphaChangeRate;
    private List<WordoCell> cellsInRow = new List<WordoCell>();

    public void AddCell(WordoCell cell)
    {
        cellsInRow.Add(cell);
    }

    public List<WordoCell> GetCellsInRow()
    {
        return cellsInRow;
    }

    public IEnumerator ChangeAlpha(float target)
    {
        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(mainCV, target, alphaChangeRate));
    }

    public IEnumerator ChangeScale(float target)
    {
        foreach (WordoCell cell in cellsInRow)
        {
            StartCoroutine(cell.ChangeScale(target));
            yield return null;
        }
    }
}
