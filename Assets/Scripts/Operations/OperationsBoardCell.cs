using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System;

public class OperationsBoardCell : InputBoardCell
{
    private OperationsGameManager activeSudokuGameManager;
    private OperationsGameManager activeManager
    {
        get
        {
            if (activeSudokuGameManager == null)
                activeSudokuGameManager = (OperationsGameManager)MiniGameManager._Instance;
            return activeSudokuGameManager;
        }
    }

    private Action<OperationsBoardCell> OnPressed;
    private MathematicalOperation cellOperation;
    public OperationsBoardCellType CellType { get; private set; }

    public MathematicalOperation GetCellOperation()
    {
        return cellOperation;
    }


    public void OnPress()
    {
        OnPressed?.Invoke(this);
    }

    public void AddOnPressedAction(Action<OperationsBoardCell> action)
    {
        OnPressed += action;
    }


    public void SetBlank()
    {
        SetCellType(OperationsBoardCellType.BLANK);
        SetSymbolAlpha(0);
        StartCoroutine(ChangeCoverAlpha(1));
        SetCoverColor(Color.black);
        Lock();
    }

    public void SetCellType(OperationsBoardCellType type)
    {
        CellType = type;
    }

    public void SetOperation(MathematicalOperation operation)
    {
        char c = ' ';
        switch (operation)
        {
            case MathematicalOperation.ADD:
                c = '+';
                break;
            case MathematicalOperation.SUBTRACT:
                c = '-';
                break;
            case MathematicalOperation.MULTIPLY:
                c = '×';
                break;
            case MathematicalOperation.DIVIDE:
                c = '÷';
                break;
        }
        cellOperation = operation;
        SetCorrectChar(c);
        SetInputtedChar(c);
    }

    public void SetAnswer(int v)
    {
        SetCellType(OperationsBoardCellType.ANSWER);
        SetCorrectChar('0');
        SetEnteredText(v.ToString());
    }

    public override void Clear()
    {
        SetOperation(MathematicalOperation.NONE);
        SetInputtedChar('0');
        SetCorrectChar('0');
    }
}