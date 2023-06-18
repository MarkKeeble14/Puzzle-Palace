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
    private List<char> pencilledChars = new List<char>();
    [SerializeField] private List<TextMeshProUGUI> pencilledCharTexts = new List<TextMeshProUGUI>();
    private Dictionary<char, TextMeshProUGUI> assignedCharTextsDict = new Dictionary<char, TextMeshProUGUI>();

    private SudokuGameManager activeConnectFourManager;
    private SudokuGameManager activeManager
    {
        get
        {
            if (activeConnectFourManager == null)
                activeConnectFourManager = (SudokuGameManager)MiniGameManager._Instance;
            return activeConnectFourManager;
        }
    }

    private bool isSelected;
    [SerializeField] private Image border;

    [SerializeField] private Color selectedBorderColor;
    [SerializeField] private Color notSelectedBorderColor;

    private Action<SudokuBoardCell> OnPressed;

    protected new void Start()
    {
        base.Start();
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

        Debug.Log("Pencilling: " + s);

        // First check if the char has already been pencilled in
        if (pencilledChars.Contains(s))
        {
            Debug.Log("Pencilled Chars Already Contains String");
            // if so, instead of adding the char, we will remove it
            pencilledChars.Remove(s);
            assignedCharTextsDict[s].gameObject.SetActive(false);
            assignedCharTextsDict.Remove(s);
            return;
        }

        // Char has not already been pencilled in, find first empty text and do so
        if (pencilledChars.Count < pencilledCharTexts.Count)
        {
            Debug.Log("Adding Pencilled Char");

            foreach (TextMeshProUGUI text in pencilledCharTexts)
            {
                Debug.Log("Check: " + text);
                if (!text.gameObject.activeInHierarchy)
                {
                    Debug.Log("1");
                    text.gameObject.SetActive(true);
                    text.text = s.ToString();
                    pencilledChars.Add(s);
                    assignedCharTextsDict.Add(s, text);
                    return;
                }
                else
                {
                    Debug.Log("2");
                }
            }
        }
    }

    private void ClearPencilledChars()
    {
        while (pencilledChars.Count > 0)
        {
            char c = pencilledChars[0];
            pencilledChars.Remove(c);
            assignedCharTextsDict[c].gameObject.SetActive(false);
            assignedCharTextsDict.Remove(c);
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
}