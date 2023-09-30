using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputBoardCell : BoardCell
{
    public static char DefaultChar = '0';
    protected char correctChar;
    protected char inputtedChar;
    protected List<char> pencilledChars = new List<char>();
    private bool locked;

    [Header("Colors")]
    [SerializeField] private Color lockedTextColor;
    [SerializeField] private Color selectedBorderColor;
    [SerializeField] private Color notSelectedBorderColor;

    [Header("References")]
    [SerializeField] private Image border;
    [SerializeField] private TextMeshProUGUI entered;
    [SerializeField] private List<PencilledCharDisplay> pencilledCharDisplays = new List<PencilledCharDisplay>();
    private Dictionary<char, PencilledCharDisplay> assignedCharTextsDict = new Dictionary<char, PencilledCharDisplay>();

    private Action<InputBoardCell> OnPressed;

    private bool isSelected;

    public static bool _Autocheck { get; set; }

    public static void _ToggleAutocheck()
    {
        _Autocheck = !_Autocheck;
    }

    protected void Start()
    {
        SetBorderColor();
    }

    public void Lock()
    {
        locked = true;
        entered.color = lockedTextColor;
    }

    protected void SetBorderColor()
    {
        border.color = isSelected ? selectedBorderColor : notSelectedBorderColor;
    }

    public void Check()
    {
        if (!correctChar.Equals(DefaultChar) && !inputtedChar.Equals(' '))
        {
            StartCoroutine(ChangeCoverAlpha(.5f));
            if (inputtedChar.Equals(correctChar))
            {
                SetCoverColor(Color.green);
            }
            else
            {
                SetCoverColor(Color.red);
            }
        }
    }

    public void SetInputtedChar(char s)
    {
        if (locked) return;

        ClearPencilledChars();
        inputtedChar = s.ToString().ToUpper().ToCharArray()[0];
        entered.text = inputtedChar.ToString();

        if (_Autocheck)
        {
            Check();
        }
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

    public virtual void Clear()
    {
        SetInputtedChar(' ');
        SetCorrectChar(DefaultChar);
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

    public void SetAllPencilledCharAlpha(float alpha)
    {
        for (int i = 0; i < pencilledCharDisplays.Count; i++)
        {
            pencilledCharDisplays[i].SetAlpha(alpha);
        }
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

    public void SetSymbolColor(Color c)
    {
        symbol.color = c;
    }

    public void OnInputPressed()
    {
        OnPressed?.Invoke(this);
    }

    public void AddOnPressedAction(Action<InputBoardCell> action)
    {
        OnPressed += action;
    }

    public void SetSymbolAlpha(float alpha)
    {
        symbolCV.alpha = alpha;
    }

    protected void SetEnteredText(string s)
    {
        entered.text = s;
    }
}
