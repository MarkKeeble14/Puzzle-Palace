using UnityEngine;

public abstract class AudioClipContainer
{
    public abstract AudioClip Clip { get; }
    [SerializeField] private float volume = 1;
    public float Volume => volume;
    [SerializeField] private float pitch = 1;
    public float Pitch => pitch;
    [SerializeField] private AudioSource source;
    public AudioSource Source { get { return source; } set { source = value; } }

    [SerializeField] private bool randomizeVolume;
    [SerializeField] private Vector2 minMaxVolume;
    [SerializeField] private bool randomizePitch;
    [SerializeField] private Vector2 minMaxPitch;

    public void SetRandoms()
    {
        if (randomizePitch)
        {
            source.pitch = RandomHelper.RandomFloat(minMaxPitch);
        }
        else
        {
            source.pitch = pitch;
        }

        if (randomizeVolume)
        {
            volume = RandomHelper.RandomFloat(minMaxVolume);
        }
        else
        {
            source.volume = volume;
        }
    }

    public void PlayOneShot()
    {
        if (!source) return;
        if (!Clip) return;
        SetRandoms();
        source.PlayOneShot(Clip, volume);
    }

    public void PlayWithPitchAdjustment(float pitchChange)
    {
        if (!source) return;
        if (!Clip) return;

        float defaultPitch = source.pitch;
        source.pitch += pitchChange;
        source.PlayOneShot(Clip, volume);
        source.pitch = defaultPitch;
    }

    public void Play()
    {
        if (!source) return;
        if (!Clip) return;
        SetRandoms();
        source.clip = Clip;
        source.Play();
    }
}
