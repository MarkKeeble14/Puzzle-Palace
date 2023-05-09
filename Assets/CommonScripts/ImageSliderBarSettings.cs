using UnityEngine;

[System.Serializable]
public struct ImageSliderBarSettings
{
    public ImageSliderBarMode Mode;
    public Color Color;
    public bool ShowWhenFull;
    public bool ShowWhenEmpty;
    public bool UseText;
    public bool ValueAsText;
    public string Prefix;
    public string Suffix;
    public bool RoundValue;
    public int DecimalPlaces;
    public FillOrigin FillOrigin;
    public float FillSpeed;
}
