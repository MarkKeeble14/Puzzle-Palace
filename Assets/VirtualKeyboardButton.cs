using System.Collections;
using UnityEngine;
using TMPro;

public abstract class VirtualKeyboardButton : MonoBehaviour
{
    [SerializeField] protected CanvasGroup cv;
    [SerializeField] protected TextMeshProUGUI text;
    [SerializeField] private float changeAlphaRate;

    protected string value;

    public void SetShown(string s)
    {
        value = s.ToUpper();
        text.text = s.ToUpper();
    }

    public abstract void OnPress();

    protected void Awake()
    {
        StartCoroutine(Utils.ChangeCanvasGroupAlpha(cv, 1, changeAlphaRate));
    }
}
