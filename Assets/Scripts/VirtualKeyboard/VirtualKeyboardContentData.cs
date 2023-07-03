
using UnityEngine;

[System.Serializable]
public class VirtualKeyboardContentData
{
    public VirtualKeyboardContentType Type;
    public string Shown;
    public Sprite Icon;
    public Vector2 SizeDelta = new Vector2(50, 60);
}
