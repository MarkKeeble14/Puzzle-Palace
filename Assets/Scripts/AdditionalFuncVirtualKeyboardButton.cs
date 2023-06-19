using UnityEngine;
using UnityEngine.UI;

public class AdditionalFuncVirtualKeyboardButton : VirtualKeyboardButton
{
    [Header("Settings")]
    [SerializeField] private float changeColorRate;
    private Color setColor;

    [Header("References")]
    [SerializeField] private Image changeColorOf;

    public override void OnPress()
    {
        ((UsesVirtualKeyboardMiniGameManager)MiniGameManager._Instance).CallAdditionalFunction(value);
    }

    private void Start()
    {
        SetState(false);
    }

    public void SetState(bool active)
    {
        if (active)
            setColor = cachedKeyboard.GetActiveButtonColor();
        else
            setColor = cachedKeyboard.GetInactiveButtonColor();
    }

    public void SetInteractable(bool active)
    {
        cv.blocksRaycasts = active;
    }

    public void SetColor(Color color)
    {
        setColor = color;
    }

    private void Update()
    {
        changeColorOf.color = Vector4.Lerp(changeColorOf.color, setColor, Time.deltaTime * changeColorRate);
    }
}
