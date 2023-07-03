using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class PencilledCharDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image background;
    [SerializeField] private CanvasGroup cv;
    public bool IsInUse { get; private set; }

    public void SetText(string t)
    {
        IsInUse = true;
        text.text = t;
    }

    public void ClearText()
    {
        IsInUse = false;
        text.text = "";
    }

    public void SetColor(Color c)
    {
        background.color = c;
    }

    public void SetAlpha(float f)
    {
        cv.alpha = f;
    }
}
