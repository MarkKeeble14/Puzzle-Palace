using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToolTipDisplay : MonoBehaviour
{
    [SerializeField] private Color confirmButtonInactiveColor;
    [SerializeField] private Color confirmButtonActiveColor;

    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image confirmButtonBackdrop;
    [SerializeField] private CanvasGroup confirmButtonCV;

    private bool confirmButtonActive;
    private Action onConfirm;
    private Action onCancel;

    public void SetState(bool b)
    {
        // Change State
        confirmButtonActive = b;

        // Change Color
        confirmButtonBackdrop.color = confirmButtonActive ? confirmButtonActiveColor : confirmButtonInactiveColor;

        // Change interactable state of Confirm Button
        confirmButtonCV.blocksRaycasts = confirmButtonActive;
    }

    public void AddOnConfirmAction(Action onConfirm)
    {
        this.onConfirm += onConfirm;
    }


    public void AddOnCancelAction(Action onCancel)
    {
        this.onCancel += onCancel;
    }

    public void Confirm()
    {
        if (confirmButtonActive)
            onConfirm?.Invoke();
    }

    public void Cancel()
    {
        onCancel?.Invoke();
    }

    public void SetText(string text)
    {
        this.text.text = text;
    }
}
