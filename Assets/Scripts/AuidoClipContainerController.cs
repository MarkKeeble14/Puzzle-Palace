using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuidoClipContainerController : MonoBehaviour
{
    [SerializeField] private SimpleAudioClipContainer toPlay;

    public void PlayOneShot()
    {
        toPlay.PlayOneShot();
    }
}
