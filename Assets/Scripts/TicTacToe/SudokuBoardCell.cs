using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System;

public class SudokuBoardCell : BoardCell
{
    private char correctChar;
    private char inputtedChar;

    [SerializeField] private TextMeshProUGUI entered;
    private List<int> pencilledChars = new List<int>();
    [SerializeField] private List<PencilledCharDisplay> pencilledCharDisplays = new List<PencilledCharDisplay>();

    private SudokuGameManager activeSudokuGameManager;
    private SudokuGameManager activeManager
    {
        get
        {
            if (activeSudokuGameManager == null)
                activeSudokuGameManager = (SudokuGameManager)MiniGameManager._Instance;
            return activeSudokuGameManager;
        }
    }

    private bool isSelected;
    [SerializeField] private Image border;

    [SerializeField] private Color selectedBorderColor;
    [SerializeField] private Color notSelectedBorderColor;

    private Action<SudokuBoardCell> OnPressed;

    private bool locked;
    [SerializeField] private Color lockedTextColor;

    public void Lock()
    {
        locked = true;
        entered.color = lockedTextColor;
    }

    public void SetSymbolColor(Color c)
    {
        symbol.color = c;
    }

    protected void Start()
    {
        SetBorderColor();
    }

    public void OnPress()
    {
        OnPressed?.Invoke(this);
    }

    public void AddOnPressedAction(Action<SudokuBoardCell> action)
    {
        OnPressed += action;
    }

    private void SetBorderColor()
    {
        border.color = isSelected ? selectedBorderColor : notSelectedBorderColor;
    }

    public void SetSymbolAlpha(float alpha)
    {
        symbolCV.alpha = alpha;
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

    public TryPencilCharResult TryPencilChar(char s)
    {
        if (locked) return TryPencilCharResult.FAIL;

        s = s.ToString().ToUpper().ToCharArray()[0];

        int index;
        if (!int.TryParse(s.ToString(), out index))
        {
            return TryPencilCharResult.FAIL;
        }

        // Debug.Log("Pencilling: " + index);

        // First check if the char has already been pencilled in
        if (pencilledChars.Contains(index))
        {
            // Debug.Log("Pencilled Chars Already Contains String");
            // if so, instead of adding the char, we will remove it
            pencilledCharDisplays[index - 1].ClearText();
            pencilledChars.Remove(index);
            return TryPencilCharResult.REMOVE;
        }

        // Char has not already been pencilled in
        if (pencilledChars.Count < pencilledCharDisplays.Count)
        {
            // Debug.Log("Adding Pencilled Char");
            pencilledChars.Add(index);
            pencilledCharDisplays[index - 1].SetText(index.ToString());
            return TryPencilCharResult.ADD;
        }
        return TryPencilCharResult.FAIL;
    }

    public void TrySetPencilledCharAlpha(char v, float alpha)
    {

        int index = ConvertCharToValue(v);
        // Debug.Log("Checking if : " + ToString() + " Contains: " + v + ", " + index);
        if (pencilledChars.Contains(index))
        {
            // Debug.Log("Contained: " + v);
            pencilledCharDisplays[index - 1].SetAlpha(alpha);
        }
    }

    public void SetAllPencilledCharAlpha(float alpha)
    {
        for (int i = 0; i < pencilledCharDisplays.Count; i++)
        {
            pencilledCharDisplays[i].SetAlpha(alpha);
        }
    }

    private int ConvertCharToValue(char v)
    {
        int index;
        if (int.TryParse(v.ToString(), out index))
        {
            return index;
        }
        return -1;
    }

    public List<int> GetPencilledChars()
    {
        return pencilledChars;
    }

    public bool HasCharPencilled(char v)
    {
        return pencilledChars.Contains(ConvertCharToValue(v));
    }

    public void PencilChar(char s)
    {
        if (locked) return;

        if (pencilledChars.Contains(s))
        {
            return;
        }

        int index;
        if (!int.TryParse(s.ToString(), out index))
        {
            return;
        }

        pencilledChars.Add(index);
        pencilledCharDisplays[index - 1].SetText(index.ToString());
    }

    public void ClearPencilledChars()
    {
        if (locked) return;

        while (pencilledChars.Count > 0)
        {
            int num = pencilledChars[0];
            pencilledChars.Remove(num);
            pencilledCharDisplays[num - 1].ClearText();
        }
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
        return inputtedChar.Equals(correctChar);
    }

    public void TryRemovePencilChar(char v)
    {
        if (locked) return;

        int x;
        if (int.TryParse(v.ToString(), out x))
        {
            if (pencilledChars.Contains(x))
            {
                pencilledChars.Remove(x);
                pencilledCharDisplays[x - 1].ClearText();
            }
        }
    }
}
