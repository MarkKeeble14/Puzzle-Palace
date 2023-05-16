using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenAnimationController : MonoBehaviour
{
    [SerializeField] private CanvasGroup fader;

    private float alphaTarget;
    [SerializeField] private float alphaChangeRate = 1.0f;

    [SerializeField] private float startingAlpha;
    [SerializeField] private float raycastBlockingThreshold = 0.75f;

    private void Awake()
    {
        alphaTarget = startingAlpha;
        fader.alpha = alphaTarget;
    }

    private void Update()
    {
        fader.alpha = Mathf.MoveTowards(fader.alpha, alphaTarget, Time.deltaTime * alphaChangeRate);
        fader.blocksRaycasts = fader.alpha > raycastBlockingThreshold;
    }

    public void Fade(float alphaAmount)
    {
        alphaTarget = alphaAmount;
    }
}
