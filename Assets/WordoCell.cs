using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordoCell : MonoBehaviour
{
    private string word;
    private char correctChar;

    private char inputtedChar;
    private List<char> pencilledChars = new List<char>();
    [SerializeField] private List<TextMeshProUGUI> pencilledCharTexts = new List<TextMeshProUGUI>();
    private Dictionary<char, TextMeshProUGUI> assignedCharTextsDict = new Dictionary<char, TextMeshProUGUI>();
    [SerializeField] private float changeColorRate;

    [SerializeField] private Color correctColor;
    [SerializeField] private Color partiallyCorrectColor;
    [SerializeField] private Color incorrectColor;

    [SerializeField] private Color selectedBorderColor;
    [SerializeField] private Color notSelectedBorderColor;

    [Header("References")]
    [SerializeField] private Image background;
    [SerializeField] private Image border;
    [SerializeField] private TextMeshProUGUI text;

    private Action<WordoCell> OnPressed;
    private bool isSelected;

    [SerializeField] private string onCorrect = "wc_onCorrect";
    [SerializeField] private string onPartialSuccess = "wc_onPartiallyCorrect";
    [SerializeField] private string onIncorrect = "wc_onIncorrect";


    public void OnPress()
    {
        OnPressed?.Invoke(this);
    }

    public void AddOnPressedAction(Action<WordoCell> action)
    {
        OnPressed += action;
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

    private void SetBorderColor()
    {
        border.color = isSelected ? selectedBorderColor : notSelectedBorderColor;
    }

    public void Set(string word, char correctChar)
    {
        this.word = word;
        this.correctChar = correctChar;
        inputtedChar = ' ';
    }

    public void SetInputtedChar(char s)
    {
        ClearPencilledChars();
        inputtedChar = s.ToString().ToUpper().ToCharArray()[0];
        text.text = inputtedChar.ToString();
    }

    public void TryPencilChar(char s)
    {
        s = s.ToString().ToUpper().ToCharArray()[0];

        // First check if the char has already been pencilled in
        if (pencilledChars.Contains(s))
        {
            // if so, instead of adding the char, we will remove it
            pencilledChars.Remove(s);
            assignedCharTextsDict[s].gameObject.SetActive(false);
            assignedCharTextsDict.Remove(s);
            return;
        }

        // Char has not already been pencilled in, find first empty text and do so
        if (pencilledChars.Count < pencilledCharTexts.Count)
        {
            foreach (TextMeshProUGUI text in pencilledCharTexts)
            {
                if (!text.gameObject.activeInHierarchy)
                {
                    text.gameObject.SetActive(true);
                    text.text = s.ToString();
                    pencilledChars.Add(s);
                    assignedCharTextsDict.Add(s, text);
                    return;
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

    public bool IsEmpty()
    {
        return inputtedChar == ' ';
    }

    public char GetInputtedChar()
    {
        return inputtedChar;
    }

    public char GetCorrectChar()
    {
        return correctChar;
    }

    public bool HasCorrectChar()
    {
        return inputtedChar.Equals(correctChar);
    }

    public IEnumerator SetResult(WordoCellResult result)
    {
        switch (result)
        {
            case WordoCellResult.CORRECT:
                AudioManager._Instance.PlayFromSFXDict(onCorrect);
                // if correct character, green
                yield return StartCoroutine(ChangeBackgroundColor(correctColor));
                break;
            case WordoCellResult.PARTIAL_CORRECT:
                AudioManager._Instance.PlayFromSFXDict(onPartialSuccess);
                // if incorrect character, but character exists in word, yellow
                yield return StartCoroutine(ChangeBackgroundColor(partiallyCorrectColor));
                break;
            case WordoCellResult.INCORRECT:
                AudioManager._Instance.PlayFromSFXDict(onIncorrect);
                // if incorrect character, and character does not exist in word, red
                yield return StartCoroutine(ChangeBackgroundColor(incorrectColor));
                break;
        }
    }

    private IEnumerator ChangeBackgroundColor(Color target)
    {
        yield return StartCoroutine(Utils.ChangeColor(background, target, changeColorRate));
    }
}
