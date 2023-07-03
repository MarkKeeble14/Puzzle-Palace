using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrosswordBoardCell : BoardCell
{
    public static char DefaultChar = '0';
    private char correctChar;
    private char inputtedChar;
    private List<char> pencilledChars = new List<char>();
    private List<CrosswordCluePlacementData> reservedBy = new List<CrosswordCluePlacementData>();

    [Header("Colors")]
    [SerializeField] private Color selectedBorderColor;
    [SerializeField] private Color notSelectedBorderColor;
    [SerializeField] private Color lockedTextColor;

    private Action<CrosswordBoardCell> OnPressed;
    private bool isSelected;
    private bool locked;

    [Header("References")]
    [SerializeField] private Image border;
    [SerializeField] private TextMeshProUGUI entered;
    [SerializeField] private List<PencilledCharDisplay> pencilledCharDisplays = new List<PencilledCharDisplay>();
    private Dictionary<char, PencilledCharDisplay> assignedCharTextsDict = new Dictionary<char, PencilledCharDisplay>();

    protected void Start()
    {
        SetBorderColor();
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

    public void AddOnPressedAction(Action<CrosswordBoardCell> action)
    {
        OnPressed += action;
    }

    public void OnPress()
    {
        OnPressed?.Invoke(this);
    }

    public void Select()
    {
        isSelected = true;
        SetBorderColor();
    }

    public void Deselect()
    {
        isSelected = false;
        SetBorderColor();
    }

    public void SetInputtedChar(char s)
    {
        if (locked) return;

        ClearPencilledChars();
        inputtedChar = s.ToString().ToUpper().ToCharArray()[0];
        entered.text = inputtedChar.ToString();
    }

    public List<char> GetPencilledChars()
    {
        return pencilledChars;
    }

    public char GetInputtedChar()
    {
        return inputtedChar;
    }

    public char GetCorrectChar()
    {
        return correctChar;
    }

    public void SetCorrectChar(char c)
    {
        correctChar = c;
    }

    public bool HasCorrectChar()
    {
        return inputtedChar.Equals(correctChar.ToString().ToUpper()[0]);
    }

    public void SetBlank()
    {
        SetSymbolAlpha(0);
        StartCoroutine(ChangeCoverAlpha(1));
        SetCoverColor(Color.black);
        Lock();
    }

    public void Clear()
    {
        SetInputtedChar(' ');
        SetCorrectChar(DefaultChar);
    }

    public void Lock()
    {
        locked = true;
        entered.color = lockedTextColor;
    }

    public bool HasCharPencilled(char v)
    {
        return pencilledChars.Contains(v);
    }

    public TryPencilCharResult TryPencilChar(char s)
    {
        if (locked) return TryPencilCharResult.FAIL;

        // Debug.Log("Pencilling: " + index);

        // First check if the char has already been pencilled in
        if (pencilledChars.Contains(s))
        {
            // Debug.Log("Pencilled Chars Already Contains String");
            // if so, instead of adding the char, we will remove it
            PencilledCharDisplay display = assignedCharTextsDict[s];
            display.ClearText();
            assignedCharTextsDict.Remove(s);
            pencilledChars.Remove(s);
            return TryPencilCharResult.REMOVE;
        }

        // Char has not already been pencilled in
        if (pencilledChars.Count < pencilledCharDisplays.Count)
        {
            // Debug.Log("Adding Pencilled Char");
            pencilledChars.Add(s);

            // Get next pencilled char
            PencilledCharDisplay display = GetNextEmptyPencilledCharDisplay();
            assignedCharTextsDict.Add(s, display);
            display.SetText(s.ToString());
            return TryPencilCharResult.ADD;
        }
        return TryPencilCharResult.FAIL;
    }

    private PencilledCharDisplay GetNextEmptyPencilledCharDisplay()
    {
        for (int i = 0; i < pencilledCharDisplays.Count; i++)
        {
            if (!pencilledCharDisplays[i].IsInUse)
            {
                return pencilledCharDisplays[i];
            }
        }
        return null;
    }

    public void ClearPencilledChars()
    {
        if (locked) return;

        while (pencilledChars.Count > 0)
        {
            char c = pencilledChars[0];
            pencilledChars.Remove(c);

            PencilledCharDisplay display = assignedCharTextsDict[c];
            assignedCharTextsDict.Remove(c);
            display.ClearText();
        }
    }

    public void TryRemovePencilChar(char c)
    {
        if (locked) return;

        if (pencilledChars.Contains(c))
        {
            pencilledChars.Remove(c);

            PencilledCharDisplay display = assignedCharTextsDict[c];
            assignedCharTextsDict.Remove(c);
            display.ClearText();
        }
    }

    public void TrySetPencilledCharAlpha(char c, float alpha)
    {
        if (pencilledChars.Contains(c))
        {
            // Debug.Log("Contained: " + v);
            PencilledCharDisplay display = assignedCharTextsDict[c];
            display.SetAlpha(alpha);
        }
    }

    public void SetAllPencilledCharAlpha(float alpha)
    {
        for (int i = 0; i < pencilledCharDisplays.Count; i++)
        {
            pencilledCharDisplays[i].SetAlpha(alpha);
        }
    }

    private void SetBorderColor()
    {
        border.color = isSelected ? selectedBorderColor : notSelectedBorderColor;
    }

    public void SetSymbolAlpha(float alpha)
    {
        symbolCV.alpha = alpha;
    }

    public void SetSymbolColor(Color c)
    {
        symbol.color = c;
    }

    public override string ToString()
    {
        return "CrosswordBoardCell: " + Coordinates;
    }
}
