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
    private AdditionalFuncVirtualKeyboardButton spawnToolTipsButton;
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

    private void CheckBoard()
    {
        board.ActOnEachBoardCell(cell =>
        {
            if (!cell.GetCorrectChar().Equals(CrosswordBoardCell.DefaultChar) && !cell.GetInputtedChar().Equals(' '))
            {
                StartCoroutine(cell.ChangeCoverAlpha(.5f));
                if (cell.GetInputtedChar().Equals(cell.GetCorrectChar()))
                {
                    cell.SetCoverColor(Color.green);
                }
                else
                {
                    cell.SetCoverColor(Color.red);
                }
            }
        });
    }

    protected override IEnumerator GameLoop()
    {
        if (!hasDealtWIthFunctionButtons)
        {
            hasDealtWIthFunctionButtons = true;

            pencilButton = virtualKeyboard.GetAdditionalFuncButton("PENCIL");
            additionalFunctionsDict.Add("PENCIL", ToggleInputMode);

            // Tool Tips
            List<ToolTipDataContainer> toolTips = new List<ToolTipDataContainer>();
            toolTips.Add(new ToolTipDataContainer("Pencil all 9 Digits into Every Cell", () => CallToolTipFunc(FullyPencilInBoard), null));
            toolTips.Add(new ToolTipDataContainer("Pencil all Possibilities into Every Cell According to Board State", () => CallToolTipFunc(CorrectlyPencilInBoard), null));
            toolTips.Add(new ToolTipDataContainer("Check the Board for Errors", () => CallToolTipFunc(CheckBoard), null));
            toolTips.Add(new ToolTipDataContainer("Solve the Selected Cell", () => CallToolTipFunc(SolveSelectedCell), null));
            toolTips.Add(new ToolTipDataContainer("Solve the Board", () => CallToolTipFunc(SolveBoard), null));

            spawnToolTipsButton = virtualKeyboard.GetAdditionalFuncButton("SPAWN_TOOLTIPS");
            additionalFunctionsDict.Add("SPAWN_TOOLTIPS", delegate
            {
                if (spawnedToolTips.Count > 0)
                {
                    Destroy(spawnedToolTips[0].transform.parent.parent.gameObject);
                    spawnedToolTips.Clear();
                }
                else
                {
                    spawnedToolTips = SpawnToolTips(toolTips);
                    // Returned order should be the same as spawned
                    spawnedToolTips[0].SetState(true);
                    spawnedToolTips[1].SetState(true);
                    spawnedToolTips[2].SetState(true);
                    spawnedToolTips[3].SetState(selectedCell != null);
                    spawnedToolTips[4].SetState(true);
                }
            });
        }

        string currentFrameString;
        while (true)
        {
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
                                        checkCell.SetCoverColor(Color.red);

                                        // Invalid Case
                                        yield return StartCoroutine(checkCell.ChangeCoverAlpha(1));

                                        yield return StartCoroutine(checkCell.ChangeCoverAlpha(0));
                                    }
                                    else if ((checkCell = board.CheckIfColContainsChar(c, selectedCell.Coordinates.y)) != null)
                                    {
                                        checkCell.SetCoverColor(Color.red);

                                        // Invalid Case
                                        yield return StartCoroutine(checkCell.ChangeCoverAlpha(1));

                                        yield return StartCoroutine(checkCell.ChangeCoverAlpha(0));

                                    }
                                    else if ((checkCell = board.CheckIfRegionContainsChar(c, selectedCell.Coordinates.x, selectedCell.Coordinates.y)) != null)
                                    {
                                        checkCell.SetCoverColor(Color.red);

                                        // Invalid Case
                                        yield return StartCoroutine(checkCell.ChangeCoverAlpha(1));

                                        yield return StartCoroutine(checkCell.ChangeCoverAlpha(0));
                                    }
                                    else
                                    {
                                        // Valid Input
                                        board.UnshowCellsWithChar();
                                        board.RemoveCharFromInvalidLocations(selectedCell, x.ToString()[0]);

                                        selectedCell.SetCoverColor(Color.white);
                                        StartCoroutine(selectedCell.ChangeCoverAlpha(0));

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

                                    selectedCell.SetCoverColor(Color.white);
                                    StartCoroutine(selectedCell.ChangeCoverAlpha(0));

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
