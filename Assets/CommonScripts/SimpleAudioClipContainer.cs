using UnityEngine;

[System.Serializable]
public class SimpleAudioClipContainer : AudioClipContainer
{
    [SerializeField] private AudioClip clip;
    public override AudioClip Clip => clip;
}
