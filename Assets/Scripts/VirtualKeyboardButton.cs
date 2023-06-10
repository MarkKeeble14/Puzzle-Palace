using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public abstract class VirtualKeyboardButton : MonoBehaviour
{
    [SerializeField] protected CanvasGroup cv;
    [SerializeField] protected TextMeshProUGUI text;
    [SerializeField] private Image image;
    [SerializeField] private float changeAlphaRate;

    protected string value;

    public void Set(string label, Sprite sprite)
    {
        value = label.ToUpper();
        text.text = label.ToUpper();

        if (sprite)
        {
            text.gameObject.SetActive(false);
            image.sprite = sprite;
            image.gameObject.SetActive(true);
        }
    }

    public abstract void OnPress();

    protected void Awake()
    {
        StartCoroutine(Utils.ChangeCanvasGroupAlpha(cv, 1, changeAlphaRate));
    }
}
