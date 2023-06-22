using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SudokuGameManager : UsesVirtualKeyboardMiniGameManager
{
    [Header("Spawning")]
    [SerializeField] private Transform parentSpawnedTo;
    [SerializeField] private SudokuBoard sudokuBoardPrefab;
    private SudokuBoard board;
    private SudokuBoardCell selectedCell;

    [Header("References")]
    [SerializeField] private ManipulateRectTransformOnMouseInput manipGamePlace;
    [SerializeField] private RectTransform scrollViewRect;

    [Header("Audio")]
    [SerializeField] private string onInput = "gm_onInput";

    [SerializeField] private Vector2Int minMaxNumHoles = new Vector2Int(60, 64);

    private AdditionalFuncVirtualKeyboardButton pencilButton;
    private AdditionalFuncVirtualKeyboardButton solveCellButton;

    private AdditionalFuncVirtualKeyboardButton solveBoardButton;
    private AdditionalFuncVirtualKeyboardButton fullyPencilButton;
    private AdditionalFuncVirtualKeyboardButton correctlyPencilButton;

    private bool gameHasBeenRestarted;

    private InputMode currentInputMode;
    private bool hasDealtWIthFunctionButtons;

    public bool AllowMove { get; protected set; }
    [SerializeField] private float delayBetweenCellsInRestartSequence;
    [SerializeField] protected float delayOnRestart = 1.0f;

    [SerializeField] private List<int> allowedNums = new List<int>();
    private bool forceChange;

    [SerializeField] protected TextMeshProUGUI timeTakenText;
    [SerializeField] protected TextMeshProUGUI hsTimeTakenText;

    protected override IEnumerator Setup()
    {
        // Generate the board
        board = Instantiate(sudokuBoardPrefab, parentSpawnedTo);

        yield return StartCoroutine(board.Generate(SelectCell, allowedNums, minMaxNumHoles));

        if (hasDealtWIthFunctionButtons)
        {
            solveCellButton.SetState(false);
            solveCellButton.SetInteractable(false);
        }

        yield return StartCoroutine(ShowKeyboard());

        if (gameHasBeenRestarted)
            gameStarted = true;
    }

    protected override IEnumerator Restart()
    {
        StartCoroutine(HideKeyboard());

        gameHasBeenRestarted = true;
        AllowMove = false;

        yield return StartCoroutine(board.ActOnEachBoardCellWithDelay(cell =>
        {
            StartCoroutine(cell.ChangeScale(0));
            cell.SetInteractable(false);
        }, delayBetweenCellsInRestartSequence, true));

        yield return new WaitForSeconds(delayOnRestart);

        Destroy(board.gameObject);

        AllowMove = true;
    }

    protected override IEnumerator GameWon()
    {
        SetTimerHighScore(hsTimeTakenText);

        timeTakenText.text = "Time to Solve: " + Utils.ParseDuration((int)timer);

        yield return null;
    }

    public enum InputMode
    {
        INPUT,
        PENCIL,
    }

    private new void Awake()
    {
        base.Awake();
    }

    private new void Update()
    {
        base.Update();
    }

    private void SelectCell(SudokuBoardCell cell)
    {
        if (selectedCell)
        {
            board.UnshowCellsWithChar();
            selectedCell.Deselect();
        }
        selectedCell = cell;
        selectedCell.Select();

        if (!selectedCell.GetInputtedChar().Equals(' '))
        {
            board.ShowCellsWithChar(selectedCell.GetInputtedChar());
        }

        List<int> pencilledChars = selectedCell.GetPencilledChars();
        foreach (int i in allowedNums)
        {
            virtualKeyboard.BlackoutKey(i.ToString(), pencilledChars.Contains(i));
        }
    }

    private void ToggleInputMode()
    {
        switch (currentInputMode)
        {
            case InputMode.INPUT:
                currentInputMode = InputMode.PENCIL;
                pencilButton.SetState(true);
                break;
            case InputMode.PENCIL:
                currentInputMode = InputMode.INPUT;
                pencilButton.SetState(false);
                break;
        }
    }

    private void ConfirmSolveSelectedCell()
    {
        SpawnToolTip("Solve the Selected Cell", SolveSelectedCell, null);
    }

    private void ConfirmSolveBoard()
    {
        SpawnToolTip("Solve the Board", SolveBoard, null);
    }

    private void ConfirmFullyPencilInBoard()
    {
        SpawnToolTip("Pencil in all Possible Options", FullyPencilInBoard, null);
    }

    private void ConfirmCorrectlyPencilInBoard()
    {
        SpawnToolTip("Pencil in all Correct Options", CorrectlyPencilInBoard, null);
    }


    private void SolveSelectedCell()
    {
        if (selectedCell)
        {
            char c = selectedCell.GetCorrectChar();
            selectedCell.SetInputtedChar(c);
            forceChange = true;

            board.RemoveCharFromInvalidLocations(selectedCell, c);
            BlackoutPencilledCharsOfCell(selectedCell, true);
        }
    }

    private void SolveBoard()
    {
        board.CheatBoard();
        forceChange = true;
    }

    private void FullyPencilInBoard()
    {
        board.FullyPencilInUnfilled(allowedNums);
        if (selectedCell)
            BlackoutPencilledCharsOfCell(selectedCell, true);
    }

    private void CorrectlyPencilInBoard()
    {
        board.CorrectlyPencilInUnfilled(allowedNums);
        if (selectedCell)
            BlackoutPencilledCharsOfCell(selectedCell, true);
    }

    private void BlackoutPencilledCharsOfCell(SudokuBoardCell cell, bool b)
    {
        List<int> pencilledChars = cell.GetPencilledChars();

        for (int i = 0; i < allowedNums.Count; i++)
        {
            int num = allowedNums[i];
            if (pencilledChars.Contains(num))
            {
                virtualKeyboard.BlackoutKey(num.ToString(), b);
            }
            else
            {
                virtualKeyboard.BlackoutKey(num.ToString(), !b);
            }
        }
    }

    protected override IEnumerator GameLoop()
    {
        if (!hasDealtWIthFunctionButtons)
        {
            hasDealtWIthFunctionButtons = true;

            pencilButton = virtualKeyboard.GetAdditionalFuncButton("PENCIL");
            additionalFunctionsDict.Add("PENCIL", ToggleInputMode);

            solveCellButton = virtualKeyboard.GetAdditionalFuncButton("SOLVE_CELL");
            additionalFunctionsDict.Add("SOLVE_CELL", ConfirmSolveSelectedCell);

            solveBoardButton = virtualKeyboard.GetAdditionalFuncButton("SOLVE_BOARD");
            additionalFunctionsDict.Add("SOLVE_BOARD", ConfirmSolveBoard);
            solveBoardButton.SetState(true);

            fullyPencilButton = virtualKeyboard.GetAdditionalFuncButton("FULLY_PENCIL");
            additionalFunctionsDict.Add("FULLY_PENCIL", ConfirmFullyPencilInBoard);
            fullyPencilButton.SetState(true);

            correctlyPencilButton = virtualKeyboard.GetAdditionalFuncButton("CORRECTLY_PENCIL");
            additionalFunctionsDict.Add("CORRECTLY_PENCIL", ConfirmCorrectlyPencilInBoard);
            correctlyPencilButton.SetState(true);
        }

        string currentFrameString;
        while (true)
        {
            solveCellButton.SetState(selectedCell != null);
            solveCellButton.SetInteractable(selectedCell != null);

            if (forceChange)
            {
                if (board.CheckForWin())
                {
                    forceChange = false;
                    yield break;
                }
            }

            if (selectedCell)
            {
                if (Input.GetKeyDown(KeyCode.Backspace) || backPressed)
                {
                    AudioManager._Instance.PlayFromSFXDict(onInput);
                    backPressed = false;
                    if (!selectedCell.GetInputtedChar().Equals(' '))
                    {
                        selectedCell.SetInputtedChar(' ');
                    }
                    else
                    {
                        selectedCell.SetInputtedChar(' ');
                    }
                }
                else
                {
                    if (keyPressed)
                    {
                        currentFrameString = key;
                        keyPressed = false;
                    }
                    else
                    {
                        currentFrameString = Input.inputString;
                    }

                    if (currentFrameString.Length > 0)
                    {
                        int x;
                        if (int.TryParse(currentFrameString[0].ToString(), out x) && allowedNums.Contains(x))
                        {
                            char c = x.ToString()[0];
                            switch (currentInputMode)
                            {
                                case InputMode.INPUT:

                                    SudokuBoardCell checkCell;
                                    if ((checkCell = board.CheckIfRowContainsChar(c, selectedCell.Coordinates.x)) != null)
                                    {
                                        // Invalid Case
                                        yield return StartCoroutine(checkCell.ChangeCoverAlpha(1));

                                        yield return StartCoroutine(checkCell.ChangeCoverAlpha(0));
                                    }
                                    else if ((checkCell = board.CheckIfColContainsChar(c, selectedCell.Coordinates.y)) != null)
                                    {
                                        // Invalid Case
                                        yield return StartCoroutine(checkCell.ChangeCoverAlpha(1));

                                        yield return StartCoroutine(checkCell.ChangeCoverAlpha(0));

                                    }
                                    else if ((checkCell = board.CheckIfRegionContainsChar(c, selectedCell.Coordinates.x, selectedCell.Coordinates.y)) != null)
                                    {
                                        // Invalid Case
                                        yield return StartCoroutine(checkCell.ChangeCoverAlpha(1));

                                        yield return StartCoroutine(checkCell.ChangeCoverAlpha(0));
                                    }
                                    else
                                    {
                                        // Valid Input
                                        board.UnshowCellsWithChar();
                                        board.RemoveCharFromInvalidLocations(selectedCell, x.ToString()[0]);

                                        selectedCell.SetInputtedChar(x.ToString()[0]);
                                        AudioManager._Instance.PlayFromSFXDict(onInput);

                                        if (board.CheckForWin())
                                        {
                                            yield break;
                                        }
                                    }


                                    break;
                                case InputMode.PENCIL:
                                    board.UnshowCellsWithChar();
                                    if (!selectedCell.GetInputtedChar().Equals(' '))
                                    {
                                        selectedCell.SetInputtedChar(' ');
                                    }

                                    TryPencilCharResult resOfTryPencilChar = selectedCell.TryPencilChar(c);
                                    if (resOfTryPencilChar == TryPencilCharResult.ADD)
                                    {
                                        virtualKeyboard.BlackoutKey(c.ToString(), true);
                                    }
                                    else if (resOfTryPencilChar == TryPencilCharResult.REMOVE)
                                    {
                                        virtualKeyboard.BlackoutKey(c.ToString(), false);
                                    }

                                    AudioManager._Instance.PlayFromSFXDict(onInput);
                                    break;
                            }
                        }
                    }
                }
            }
            else
            {
                keyPressed = false;
            }
            yield return null;
        }
    }
}
