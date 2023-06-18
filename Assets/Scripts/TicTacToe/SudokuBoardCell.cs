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
    [SerializeField] private List<TextMeshProUGUI> pencilledCharTexts = new List<TextMeshProUGUI>();

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
        ClearPencilledChars();
        inputtedChar = s.ToString().ToUpper().ToCharArray()[0];
        entered.text = inputtedChar.ToString();
    }

    public void TryPencilChar(char s)
    {
        s = s.ToString().ToUpper().ToCharArray()[0];

        int index;
        if (!int.TryParse(s.ToString(), out index))
        {
            return;
        }

        // Debug.Log("Pencilling: " + index);

        // First check if the char has already been pencilled in
        if (pencilledChars.Contains(index))
        {
            // Debug.Log("Pencilled Chars Already Contains String");
            // if so, instead of adding the char, we will remove it
            pencilledCharTexts[index - 1].text = "";
            pencilledChars.Remove(index);
            return;
        }

        // Char has not already been pencilled in, find first empty text and do so
        if (pencilledChars.Count < pencilledCharTexts.Count)
        {
            // Debug.Log("Adding Pencilled Char");
            pencilledChars.Add(index);
            pencilledCharTexts[index - 1].text = index.ToString();
        }
    }

    private void ClearPencilledChars()
    {
        while (pencilledChars.Count > 0)
        {
            int num = pencilledChars[0];
            pencilledChars.Remove(num);
            pencilledCharTexts[num - 1].text = "";
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
        int x;
        if (int.TryParse(v.ToString(), out x))
        {
            if (pencilledChars.Contains(x))
            {
                pencilledChars.Remove(x);
                pencilledCharTexts[x - 1].text = "";
            }
        }
    }
}