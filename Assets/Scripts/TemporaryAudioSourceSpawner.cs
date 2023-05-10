using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporaryAudioSourceSpawner : MonoBehaviour
{
    [SerializeField] private SimpleAudioClipContainer toPlay;

    public void PlayOneShot()
    {
        TemporaryAudioSource tempSorce = Instantiate(Resources.Load<TemporaryAudioSource>("Audio/TemporaryAudioSource"));
        tempSorce.Play(toPlay);
    }
}
