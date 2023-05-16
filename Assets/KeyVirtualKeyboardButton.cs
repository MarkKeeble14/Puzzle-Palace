using UnityEngine;
using UnityEngine.UI;

public class KeyVirtualKeyboardButton : VirtualKeyboardButton
{
    [SerializeField] private Image changeColorOf;
    [SerializeField] private float changeColorRate;

    [SerializeField] private Color activeColor;
    [SerializeField] private Color blackedOutColor;

    protected new void Awake()
    {
        changeColorOf.color = activeColor;
        base.Awake();
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
