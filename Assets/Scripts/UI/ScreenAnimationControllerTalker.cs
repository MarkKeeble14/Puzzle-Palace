using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenAnimationControllerTalker : MonoBehaviour
{
    [SerializeField] private ScreenAnimationController controlling;

    public void ToggleScreenAnimationController()
    {
        if (controlling.Active)
        {
            controlling.Fade(0);
        }
        else
        {
            controlling.Fade(1);
        }
    }
}
