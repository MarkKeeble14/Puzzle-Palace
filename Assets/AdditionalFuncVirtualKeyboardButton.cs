using UnityEngine;
using UnityEngine.UI;

public class AdditionalFuncVirtualKeyboardButton : VirtualKeyboardButton
{
    [Header("Settings")]
    [SerializeField] private float changeColorRate;
    private Color setColor;

    [Header("References")]
    [SerializeField] private Image changeColorOf;

    private void Start()
    {
        SetColor(WordoDataDealer._Instance.GetInactiveButtonColor());
    }

    public override void OnPress()
    {
        ((WordoGameManager)MiniGameManager._Instance).CallAdditionalFunction(value);
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
