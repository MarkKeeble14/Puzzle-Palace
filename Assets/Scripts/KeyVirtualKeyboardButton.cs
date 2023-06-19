using UnityEngine;
using UnityEngine.UI;

public class KeyVirtualKeyboardButton : VirtualKeyboardButton
{
    [SerializeField] private Image changeColorOf;
    [SerializeField] private float changeColorRate;

    [SerializeField] private Color activeColor;
    [SerializeField] private Color blackedOutColor;

    protected void Awake()
    {
        changeColorOf.color = activeColor;
    }

    public override void OnPress()
    {
        ((UsesVirtualKeyboardMiniGameManager)MiniGameManager._Instance).KeyPressed(value);
    }

    public void Blackout(bool v)
    {
        StartCoroutine(Utils.ChangeColor(changeColorOf, v ? blackedOutColor : activeColor, changeColorRate));
        // cv.blocksRaycasts = !v;
    }
}
