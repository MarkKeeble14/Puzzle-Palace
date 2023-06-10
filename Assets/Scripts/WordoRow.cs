using System.Collections;
using UnityEngine;

public class WordoRow : MonoBehaviour
{
    [SerializeField] private CanvasGroup mainCV;
    [SerializeField] private float alphaChangeRate;

    public IEnumerator ChangeAlpha(float target)
    {
        yield return StartCoroutine(Utils.ChangeCanvasGroupAlpha(mainCV, target, alphaChangeRate));
    }
}
