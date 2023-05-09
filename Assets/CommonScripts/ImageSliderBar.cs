using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ImageSliderBar : MonoBehaviour
{
    [SerializeField] private ImageSliderBarMode mode;
    [SerializeField] private bool showWhenFull;
    [SerializeField] private bool showWhenEmpty;
    [SerializeField] private bool useText;
    [SerializeField] private string prefix;
    [SerializeField] private string suffix;
    [SerializeField] private bool roundValue;
    [SerializeField] private int decimalPlaces = 1;
    [SerializeField] private FillOrigin fillOrigin;
    [SerializeField] private float fillSpeed = 50f;

    private float maxValue = 1f;
    private float value;
    private float currentValue;
    private float displayedValue;

    public bool IsFull => displayedValue * maxValue >= maxValue * 0.99f;

    [Header("References")]
    [SerializeField] private Image bar;
    [SerializeField] private TextMeshProUGUI text;

    private Action onFullAction;

    public void Show()
    {
        displayedValue = 0;
        bar.fillAmount = 0;
        gameObject.SetActive(true);
    }

    public void SetFromSettings(ImageSliderBarSettings settings)
    {
        mode = settings.Mode;
        SetColor(settings.Color);
        useText = settings.UseText;
        showWhenFull = settings.ShowWhenFull;
        showWhenEmpty = settings.ShowWhenEmpty;
        prefix = settings.Prefix;
        suffix = settings.Suffix;
        roundValue = settings.RoundValue;
        decimalPlaces = settings.DecimalPlaces;
        fillOrigin = settings.FillOrigin;
        fillSpeed = settings.FillSpeed;
    }

    private void Update()
    {
        // Set Value and Control Object State
        currentValue = value / maxValue;
        bool isWithinMinAndMax = currentValue > 0 && value < maxValue;
        bool isAtOrOverMax = value >= maxValue && showWhenFull;
        bool isAtOrUnderMin = value <= 0;
        bool shouldBeActive = isWithinMinAndMax || (showWhenFull && isAtOrOverMax) || (showWhenEmpty && isAtOrUnderMin);

        // Debug.Log(value + ", " + maxValue + ", " + currentValue + ", " + shouldBeActive);

        // If not set to use text, don't use text
        text.gameObject.SetActive(shouldBeActive && useText);

        // Don't show the bar when either at 0 or equal to or above 1
        bar.gameObject.SetActive(shouldBeActive);

        // Set the fill origin
        bar.fillOrigin = (int)fillOrigin;

        // Need to check for this otherwise wierd stuff happens
        if (float.IsNaN(displayedValue))
        {
            displayedValue = 0;
        }

        // Actually change fill amount
        displayedValue = Mathf.Lerp(displayedValue, currentValue, Time.deltaTime * fillSpeed);
        bar.fillAmount = displayedValue;

        // Determine what text to display
        string showPercentValue = roundValue ? System.Math.Round(displayedValue * 100, decimalPlaces).ToString() : displayedValue.ToString();
        string showPercentInvertedValue = roundValue ? System.Math.Round((1 - displayedValue) * 100, decimalPlaces).ToString() : (1 - displayedValue).ToString();
        string showCompensatedValue = roundValue ? System.Math.Round(displayedValue * maxValue, decimalPlaces).ToString() : (displayedValue * maxValue).ToString();
        string showCompensatedInvertValue = roundValue ? System.Math.Round(maxValue - (displayedValue * maxValue), decimalPlaces).ToString() : (maxValue - (displayedValue * maxValue)).ToString();
        string t = "";
        switch (mode)
        {
            case ImageSliderBarMode.PERCENT:
                t = showPercentValue;
                break;
            case ImageSliderBarMode.PERCENT_INVERT:
                t = showPercentInvertedValue;
                break;
            case ImageSliderBarMode.COMPENSATED:
                t = showCompensatedValue;
                break;
            case ImageSliderBarMode.COMPENSATED_INVERT:
                t = showCompensatedInvertValue;
                break;
            default:
                throw new UnhandledSwitchCaseException();

        }
        // Set Text
        text.text = prefix + t + suffix;

        // call on full action
        if (currentValue >= 1 && value == maxValue)
        {
            onFullAction?.Invoke();
        }
    }

    public void SetValue(float v)
    {
        value = v;
    }

    public void SetMaxValue(float v)
    {
        maxValue = v;
    }

    public void Set(float current, float max)
    {
        value = current;
        maxValue = max;
    }

    public void SetColor(Color c)
    {
        bar.color = c;
    }

    public void SetHorizontalDirection(FillOrigin direction)
    {
        this.fillOrigin = direction;
    }

    public void AddOnFullAction(Action action)
    {
        onFullAction += action;
    }

    public void ClearOnFullAction()
    {
        onFullAction = null;
    }
}
