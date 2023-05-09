using System.Collections;
using UnityEngine;

public class TemporaryAudioSource : MonoBehaviour
{
    [SerializeField] private AudioSource source;

    public void Play(AudioClip clip, float volume, float pitch)
    {
        if (!gameObject.activeInHierarchy) return;
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.Play();
        StartCoroutine(DestroyWhenDone());
    }

    public void Play(AudioClipContainer clip)
    {
        if (!gameObject.activeInHierarchy) return;
        source.clip = clip.Clip;
        source.volume = clip.Volume;
        source.pitch = clip.Pitch;
        source.Play();
        StartCoroutine(DestroyWhenDone());
    }

    private IEnumerator DestroyWhenDone()
    {
        yield return new WaitUntil(() => !source.isPlaying);

        Destroy(gameObject);
    }
}
