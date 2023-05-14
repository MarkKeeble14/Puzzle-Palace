using UnityEngine;

public class AudioCaller : MonoBehaviour
{
    [SerializeField] private string key;

    public void Play()
    {
        AudioManager._Instance.PlayFromSFXDict(key);
    }
}
