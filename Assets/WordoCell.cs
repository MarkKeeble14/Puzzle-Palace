using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordoCell : MonoBehaviour
{
    private string word;
    private char correctChar;

    private char inputtedChar;
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

    [SerializeField] private SimpleAudioClipContainer onSuccess;
    [SerializeField] private SimpleAudioClipContainer onPartialSuccess;
    [SerializeField] private SimpleAudioClipContainer onFail;

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
        inputtedChar = s.ToString().ToUpper().ToCharArray()[0];
        text.text = inputtedChar.ToString();
    }

    public bool IsEmpty()
    {
        return inputtedChar == ' ';
    }

    public char GetInputtedChar()
    {
        return inputtedChar;
    }

    public bool IsCorrectGuess()
    {
        return inputtedChar.Equals(correctChar);
    }

    public bool IsPartiallyCorrectGuess()
    {
        return !IsCorrectGuess() && word.Contains(inputtedChar);
    }

    public bool IsIncorrectGuess()
    {
        return !IsCorrectGuess() && !IsPartiallyCorrectGuess();
    }

    public IEnumerator Check()
    {
        if (IsCorrectGuess())
        {
            onSuccess.PlayOneShot();
            // if correct character, green
            yield return StartCoroutine(ChangeBackgroundColor(correctColor));
        }
        else if (IsPartiallyCorrectGuess())
        {
            onPartialSuccess.PlayOneShot();
            // if incorrect character, but character exists in word, yellow
            yield return StartCoroutine(ChangeBackgroundColor(partiallyCorrectColor));
        }
        else
        {
            onFail.PlayOneShot();
            // if incorrect character, and character does not exist in word, red
            yield return StartCoroutine(ChangeBackgroundColor(incorrectColor));
        }
    }

    private IEnumerator ChangeBackgroundColor(Color target)
    {
        yield return StartCoroutine(Utils.ChangeColor(background, target, changeColorRate));
    }
}
