using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public abstract class VirtualKeyboardButton : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] protected TextMeshProUGUI text;
    protected CanvasGroup cv;
    private RectTransform rect;
    [SerializeField] private float changeAlphaRate;

    protected string value;
    protected VirtualKeyboard cachedKeyboard;

    public void Set(VirtualKeyboard kb, VirtualKeyboardContentData data)
    {
        // Get References
        rect = GetComponent<RectTransform>();
        if (!cv)
            cv = GetComponent<CanvasGroup>();

        // Store Reference
        cachedKeyboard = kb;

        value = data.Shown.ToUpper();
        text.text = data.Shown.ToUpper();
        rect.sizeDelta = data.SizeDelta;

        if (data.Icon)
        {
            text.gameObject.SetActive(false);
            image.sprite = data.Icon;
            image.gameObject.SetActive(true);
        }

        StartCoroutine(Utils.ChangeCanvasGroupAlpha(cv, 1, changeAlphaRate));
    }

    public abstract void OnPress();
}
