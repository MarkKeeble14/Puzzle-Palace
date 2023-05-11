using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeAudioSource : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private float fadeRate = 1.0f;
    [SerializeField] private float volTarget;
    [SerializeField] private bool fadeInOnStart;

    private void Awake()
    {
        if (fadeInOnStart)
        {
            CallFade(source, volTarget);
        }
    }

    public void CallFade(AudioSource source, float volTarget)
    {
        StartCoroutine(ExecuteFade(source, volTarget));
    }

    private IEnumerator ExecuteFade(AudioSource source, float volTarget)
    {
        while (source.volume != volTarget)
        {
            source.volume = Mathf.MoveTowards(source.volume, volTarget, Time.deltaTime * fadeRate);
            yield return null;
        }
    }
}
