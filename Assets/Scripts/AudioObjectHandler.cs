using UnityEngine;

public class AudioObjectHandler : MonoBehaviour
{
    private AudioSource[] persistentAudio;

    private void Awake()
    {
        // If there's only one audio object handler in the scene (this one), add it to don't destroy on load.
        // If theres more than one, destroy the new one and leave the pre-existing one
        if (FindObjectsOfType<AudioObjectHandler>().Length == 1)
        {
            DontDestroyOnLoad(gameObject);
            persistentAudio = GetComponentsInChildren<AudioSource>();
            foreach (AudioSource source in persistentAudio)
            {
                source.Play();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
