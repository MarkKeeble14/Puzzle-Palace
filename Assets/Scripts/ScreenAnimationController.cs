using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void Fade(bool v)
    {
        animator.SetBool("Fade", v);
    }
}
