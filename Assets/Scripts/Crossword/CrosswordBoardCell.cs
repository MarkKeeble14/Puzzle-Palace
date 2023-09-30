using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosswordBoardCell : InputBoardCell
{
    private List<CrosswordCluePlacementData> reservedBy = new List<CrosswordCluePlacementData>();


    private Action<CrosswordBoardCell> OnPressed;

    public void OnPress()
    {
        OnPressed?.Invoke(this);
    }

    public void AddOnPressedAction(Action<CrosswordBoardCell> action)
    {
        OnPressed += action;
    }

    public bool CanBeReserved()
    {
        return reservedBy.Count < 2;
    }

    public void SetReservedBy(CrosswordCluePlacementData data)
    {
        reservedBy.Add(data);
    }

    public void ResetReservedBy()
    {
        reservedBy.Clear();
    }

    public void RemoveReservedBy(CrosswordCluePlacementData data)
    {
        reservedBy.Remove(data);
    }

    public List<CrosswordCluePlacementData> GetReservedBy()
    {
        return reservedBy;
    }

    public void SetBlank()
    {
        SetSymbolAlpha(0);
        StartCoroutine(ChangeCoverAlpha(1));
        SetCoverColor(Color.black);
        Lock();
    }

    public override string ToString()
    {
        return "CrosswordBoardCell: " + Coordinates;
    }
}
