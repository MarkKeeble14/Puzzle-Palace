using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System;

public class SudokuBoardCell : InputBoardCell
{
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

    private Action<SudokuBoardCell> OnPressed;

    public void OnPress()
    {
        OnPressed?.Invoke(this);
    }

    public void AddOnPressedAction(Action<SudokuBoardCell> action)
    {
        OnPressed += action;
    }
}
