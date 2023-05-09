using UnityEngine;

public class ContinuousAudioSource : MonoBehaviour
{
    public bool Active { get; set; }
    [SerializeField] private AudioSource audioSource;
    private void Update()
    {
        audioSource.mute = !Active;
    }
}
