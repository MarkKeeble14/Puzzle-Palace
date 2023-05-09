using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager _Instance { get; private set; }
    private void Awake()
    {
        if (_Instance != null)
        {
            Destroy(_Instance.gameObject);
        }
        _Instance = this;
        if (fadeInOnAwake)
            FadeIn();
    }

    [SerializeField] private bool fadeInOnAwake = true;

    [SerializeField] private Animator anim;

    public void FadeOut(Action action, float timeBetween = 0)
    {
        StartCoroutine(PlayAnimationThenDoAction("FadeOut", action, timeBetween));
    }

    public void FadeOut()
    {
        anim.CrossFade("FadeOut", 0, 0);
    }

    public void FadeIn(Action action, float timeBetween = 0)
    {
        StartCoroutine(PlayAnimationThenDoAction("FadeIn", action, timeBetween));
    }

    public void FadeIn()
    {
        anim.CrossFade("FadeIn", 0, 0);
    }

    private IEnumerator PlayAnimationThenDoAction(string animationName, Action action, float timeBetween = 0)
    {
        // Debug.Log("Playing: " + animationName);
        anim.CrossFade(animationName, 0, 0);

        yield return new WaitUntil(() => AnimationHelper.AnimatorIsPlayingClip(anim, animationName));
        yield return new WaitForSecondsRealtime(timeBetween);
        yield return new WaitUntil(() => !AnimationHelper.AnimatorIsPlayingClip(anim, animationName));

        action();
    }
}
