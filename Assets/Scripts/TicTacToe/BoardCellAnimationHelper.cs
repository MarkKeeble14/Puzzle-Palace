using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCellAnimationHelper : MonoBehaviour
{
    [SerializeField] private SimpleAudioClipContainer onDestroy;
    public void DestroyGameObject()
    {
        TemporaryAudioSource tempAudioSource = Resources.Load<TemporaryAudioSource>("Audio/TemporaryAudioSource");
        tempAudioSource.Play(onDestroy);
        Destroy(gameObject);
    }
}
