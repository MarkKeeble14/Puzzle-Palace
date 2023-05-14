using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    [Header("SFX")]
    [SerializeField] private SerializableDictionary<string, SimpleAudioClipContainer> sfxDict = new SerializableDictionary<string, SimpleAudioClipContainer>();

    [Header("References")]
    [SerializeField] private AudioMixer mixer;
    private List<AudioSource> audioSourceArray;
    [SerializeField] private Transform parentSpawnedTo;
    [SerializeField] private AudioSource audioSourcePrefab;

    [Header("Settings")]
    [SerializeField] private float minPercent = 0.0001f;
    [SerializeField] private Slider musicVolumeSlider;
    private string musicVolumeKey = "MusicVolume";
    [SerializeField] private float defaultMusicVolume = 0.8f;
    private bool musicEnabled = true;
    private string musicEnabledKey = "MusicEnabled";
    [SerializeField] private Image musicActiveStateCross;

    [SerializeField] private Slider sfxVolumeSlider;
    private string sfxVolumeKey = "SFXVolume";
    [SerializeField] private float defaultSFXVolume = 0.8f;
    private bool sfxEnabled = true;
    private string sfxEnabledKey = "SFXEnabled";
    [SerializeField] private Image sfxActiveStateCross;



    public static AudioManager _Instance { get; private set; }

    private void Awake()
    {
        _Instance = this;

        audioSourceArray = new List<AudioSource>();
        audioSourceArray.AddRange(GetComponentsInChildren<AudioSource>());
    }

    private AudioSource GetAudioSource()
    {
        if (audioSourceArray.Count == 0)
        {
            AudioSource spawned = Instantiate(audioSourcePrefab, parentSpawnedTo);
            audioSourceArray.Add(spawned);
        }
        return audioSourceArray[0];
    }

    private IEnumerator PlayFromSourceUninterrupted(SimpleAudioClipContainer clip, float pitchAdjustment)
    {
        AudioSource source = GetAudioSource();

        audioSourceArray.Remove(source);
        clip.Source = source;

        clip.PlayWithPitchAdjustment(pitchAdjustment);

        yield return new WaitUntil(() => !source.isPlaying);

        audioSourceArray.Add(source);
    }

    public void PlayFromSFXDict(string key)
    {
        StartCoroutine(PlayFromSourceUninterrupted(sfxDict[key], 0.0f));
    }

    public void PlayFromSFXDict(string key, float pitchAdjustment)
    {
        StartCoroutine(PlayFromSourceUninterrupted(sfxDict[key], pitchAdjustment));
    }


    public void SetMusicVolume(float percent)
    {
        if (musicEnabled)
            mixer.SetFloat("MusicVolume", Mathf.Log10(percent) * 20);
        PlayerPrefs.SetFloat(musicVolumeKey, percent);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float percent)
    {
        if (sfxEnabled)
            mixer.SetFloat("SFXVolume", Mathf.Log10(percent) * 20);
        PlayerPrefs.SetFloat(sfxVolumeKey, percent);
        PlayerPrefs.Save();
    }


    public void ToggleMuteSFX()
    {
        sfxEnabled = !sfxEnabled;
        sfxActiveStateCross.gameObject.SetActive(!sfxEnabled);
        PlayerPrefs.SetInt(sfxEnabledKey, sfxEnabled ? 1 : 0);
        PlayerPrefs.Save();

        if (sfxEnabled)
        {
            SetSFXVolume(PlayerPrefs.GetFloat(sfxVolumeKey));
        }
        else
        {
            mixer.SetFloat("SFXVolume", Mathf.Log10(minPercent) * 20);
        }
    }

    public void ToggleMuteMusic()
    {
        musicEnabled = !musicEnabled;
        musicActiveStateCross.gameObject.SetActive(!musicEnabled);
        PlayerPrefs.SetInt(musicEnabledKey, musicEnabled ? 1 : 0);
        PlayerPrefs.Save();

        if (musicEnabled)
        {
            SetMusicVolume(PlayerPrefs.GetFloat(musicVolumeKey));
        }
        else
        {
            mixer.SetFloat("MusicVolume", Mathf.Log10(minPercent) * 20);
        }
    }

    private void Start()
    {
        // Music Volume
        if (!PlayerPrefs.HasKey(musicVolumeKey))
        {
            PlayerPrefs.SetFloat(musicVolumeKey, defaultMusicVolume);
            SetMusicVolume(defaultMusicVolume);
            musicVolumeSlider.value = defaultSFXVolume;
        }
        else
        {
            float musicVol = PlayerPrefs.GetFloat(musicVolumeKey);
            SetMusicVolume(musicVol);
            musicVolumeSlider.value = musicVol;
        }

        // SFX Volume
        if (!PlayerPrefs.HasKey(sfxVolumeKey))
        {
            PlayerPrefs.SetFloat(sfxVolumeKey, defaultSFXVolume);
            SetSFXVolume(defaultSFXVolume);
            sfxVolumeSlider.value = defaultSFXVolume;
        }
        else
        {
            float sfxVol = PlayerPrefs.GetFloat(sfxVolumeKey);
            SetSFXVolume(sfxVol);
            sfxVolumeSlider.value = sfxVol;
        }

        // Music Enabled
        if (!PlayerPrefs.HasKey(musicEnabledKey)
            || (PlayerPrefs.HasKey(musicEnabledKey) && PlayerPrefs.GetInt(musicEnabledKey) == 1))
        {
            musicEnabled = true;
            PlayerPrefs.SetInt(musicEnabledKey, 1);
            PlayerPrefs.Save();
            musicActiveStateCross.gameObject.SetActive(false);
        }
        else
        {
            // Has Key and Key is 0
            musicEnabled = false;
            musicActiveStateCross.gameObject.SetActive(true);
            mixer.SetFloat("MusicVolume", Mathf.Log10(minPercent) * 20);
        }

        // SFX Enabled
        if (!PlayerPrefs.HasKey(sfxEnabledKey)
            || (PlayerPrefs.HasKey(sfxEnabledKey) && PlayerPrefs.GetInt(sfxEnabledKey) == 1))
        {
            sfxEnabled = true;
            PlayerPrefs.SetInt(sfxEnabledKey, 1);
            PlayerPrefs.Save();
            sfxActiveStateCross.gameObject.SetActive(false);
        }
        else
        {
            // Has Key and Key is 0
            sfxEnabled = false;
            sfxActiveStateCross.gameObject.SetActive(true);
            mixer.SetFloat("SFXVolume", Mathf.Log10(minPercent) * 20);
        }
    }
}
