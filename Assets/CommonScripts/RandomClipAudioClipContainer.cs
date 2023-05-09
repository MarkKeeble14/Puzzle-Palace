using UnityEngine;

[System.Serializable]
public class RandomClipAudioClipContainer : AudioClipContainer
{
    [SerializeField] private AudioClip[] clipOptions;
    public override AudioClip Clip => RandomHelper.GetRandomFromArray(clipOptions);
}
