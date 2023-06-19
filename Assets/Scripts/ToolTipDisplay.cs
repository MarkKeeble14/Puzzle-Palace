using System;
using TMPro;
using UnityEngine;

public class ToolTipDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private Action onConfirm;

    public void AddOnConfirmAction(Action onConfirm)
    {
        this.onConfirm += onConfirm;
    }

    private Action onCancel;

    public void AddOnCancelAction(Action onCancel)
    {
        this.onCancel += onCancel;
    }

    public void Confirm()
    {
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
