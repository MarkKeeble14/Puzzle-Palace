using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class PencilledCharDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image background;
    [SerializeField] private CanvasGroup cv;

    public void SetText(string t)
    {
        text.text = t;
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
