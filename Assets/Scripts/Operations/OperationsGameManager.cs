using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OperationsGameManager : UsesVirtualKeyboardMiniGameManager
{
    [Header("Spawning")]
    [SerializeField] private Transform parentSpawnedTo;
    [SerializeField] private OperationsBoard numbersBoardPrefab;
    private OperationsBoard board;
    private OperationsBoardCell selectedCell;

    [Header("References")]
    [SerializeField] private ManipulateRectTransformOnMouseInput manipGamePlace;
    [SerializeField] private RectTransform scrollViewRect;

    [Header("Audio")]
    [SerializeField] private string onInput = "gm_onInput";

    private AdditionalFuncVirtualKeyboardButton pencilButton;
    private AdditionalFuncVirtualKeyboardButton solveCellButton;

    private AdditionalFuncVirtualKeyboardButton solveBoardButton;

    private bool gameHasBeenRestarted;

    private InputMode currentInputMode;
    private bool hasDealtWIthFunctionButtons;

    public bool AllowMove { get; protected set; }

    [SerializeField] private float delayBetweenCellsInRestartSequence;
    [SerializeField] protected float delayOnRestart = 1.0f;

    [SerializeField] private List<int> allowedNums = new List<int>();
    [SerializeField] private List<MathematicalOperation> allowedOps = new List<MathematicalOperation>();
    private bool forceChange;

    [SerializeField] protected TextMeshProUGUI timeTakenText;
    [SerializeField] protected TextMeshProUGUI hsTimeTakenText;

    protected override IEnumerator Setup()
    {
        // Generate the board
        board = Instantiate(numbersBoardPrefab, parentSpawnedTo);

        yield return StartCoroutine(board.Generate(SelectCell, allowedNums, allowedOps));

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

    private void SelectCell(OperationsBoardCell cell)
    {
        if (selectedCell)
        {
            board.UnshowCellsWithChar();
            selectedCell.Deselect();
        }
        selectedCell = cell;
        selectedCell.Select();


        if (selectedCell.CellType == OperationsBoardCellType.NUM)
        {
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


    private void SolveSelectedCell()
    {
        if (selectedCell)
        {
            char c = selectedCell.GetCorrectChar();
            board.RemoveCharFromInvalidLocations(c);

            selectedCell.SetInputtedChar(c);
            forceChange = true;

            BlackoutPencilledCharsOfCell(selectedCell, true);
        }
    }

    private void SolveBoard()
    {
        board.CheatBoard();
        forceChange = true;
    }

    private void BlackoutPencilledCharsOfCell(OperationsBoardCell cell, bool b)
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
                                    OperationsBoardCell checkCell;
                                    if ((checkCell = board.CheckIfBoardContainsChar(c)) != null)
                                    {
                                        // Invalid Case
                                        yield return StartCoroutine(checkCell.ChangeCoverAlpha(1));

                                        yield return StartCoroutine(checkCell.ChangeCoverAlpha(0));
                                    }
                                    else
                                    {
                                        // Valid Input
                                        board.UnshowCellsWithChar();
                                        board.RemoveCharFromInvalidLocations(x.ToString()[0]);

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
