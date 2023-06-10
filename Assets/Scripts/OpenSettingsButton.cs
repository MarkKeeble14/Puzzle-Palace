using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenSettingsButton : MonoBehaviour
{
    public void OnPress()
    {
        UIManager._Instance.Pause();
    }
}
