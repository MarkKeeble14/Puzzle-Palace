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
    private AdditionalFuncVirtualKeyboardButton spawnToolTipsButton;

    private bool gameHasBeenRestarted;

    private InputMode currentInputMode;
    private bool hasDealtWIthFunctionButtons;

    public bool AllowMove { get; protected set; }

    [SerializeField] private float delayBetweenCellsInRestartSequence;
    [SerializeField] protected float delayOnRestart = 1.0f;

    [SerializeField] private SerializableDictionary<MathematicalOperation, int> maxOpsDict = new SerializableDictionary<MathematicalOperation, int>();
    [SerializeField] private List<int> allowedNums = new List<int>();
    [SerializeField] private List<MathematicalOperation> allowedOps = new List<MathematicalOperation>();
    private bool forceChange;

    [SerializeField] protected TextMeshProUGUI timeTakenText;
    [SerializeField] protected TextMeshProUGUI hsTimeTakenText;

    protected override IEnumerator Setup()
    {
        // Generate the board
        board = Instantiate(numbersBoardPrefab, parentSpawnedTo);

        yield return StartCoroutine(board.Generate(SelectCell, allowedNums, allowedOps, maxOpsDict));

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

        InputBoardCell._Autocheck = false;

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

            List<char> pencilledChars = selectedCell.GetPencilledChars();
            foreach (int i in allowedNums)
            {
                char c;
                char.TryParse(i.ToString(), out c);
                virtualKeyboard.BlackoutKey(c.ToString(), pencilledChars.Contains(c));
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

    private void SolveSelectedCell()
    {
        if (selectedCell)
        {
            char c = selectedCell.GetCorrectChar();
            board.RemoveCharFromInvalidLocations(c);

            selectedCell.SetInputtedChar(c);
            forceChange = true;

            BlackoutPencilledCharsOfCell(selectedCell, true);

            selectedCell.Check();
        }
    }

    private void SolveBoard()
    {
        board.CheatBoard();
        forceChange = true;
    }

    private void BlackoutPencilledCharsOfCell(OperationsBoardCell cell, bool b)
    {
        List<char> pencilledChars = cell.GetPencilledChars();

        for (int i = 0; i < allowedNums.Count; i++)
        {
            int num = allowedNums[i];
            char c;
            char.TryParse(num.ToString(), out c);

            if (pencilledChars.Contains(c))
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
            if (cell.CellType == OperationsBoardCellType.NUM)
                cell.Check();
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
            toolTips.Add(new ToolTipDataContainer("Toggle Autocheck", () => CallToolTipFunc(InputBoardCell._ToggleAutocheck), null));
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
                    spawnedToolTips[2].SetState(selectedCell != null);
                    spawnedToolTips[3].SetState(true);
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

                    selectedCell.SetInputtedChar(' ');

                    selectedCell.SetCoverColor(Color.white);
                    StartCoroutine(selectedCell.ChangeCoverAlpha(0));
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
                                        StartCoroutine(checkCell.PulseCoverAlpha());
                                    }
                                    else
                                    {
                                        // Valid Input
                                        board.UnshowCellsWithChar();
                                        board.RemoveCharFromInvalidLocations(x.ToString()[0]);

                                        selectedCell.SetCoverColor(Color.white);

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
