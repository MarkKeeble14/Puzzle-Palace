using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AnimateLoadingText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textToAnimate;
    [SerializeField] private CanvasGroup toAnimate;
    [SerializeField] private string loadingString;
    [SerializeField] private float timeBetweenAddingLetters;
    [SerializeField] private float timeBeforeFade;
    [SerializeField] private float alphaAnimationSpeed;
    [SerializeField] private float timeBetweenAnimations;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        toAnimate.alpha = 1;
        textToAnimate.text = "";

        int index = 0;
        while (textToAnimate.text != loadingString)
        {
            textToAnimate.text += loadingString[index++];

            yield return new WaitForSeconds(timeBetweenAddingLetters);
        }

        yield return new WaitForSeconds(timeBeforeFade);

        while (toAnimate.alpha != 0)
        {
            toAnimate.alpha = Mathf.MoveTowards(toAnimate.alpha, 0, Time.deltaTime * alphaAnimationSpeed);

            yield return null;
        }

        yield return new WaitForSeconds(timeBetweenAnimations);

        StartCoroutine(Animate());
    }
}
