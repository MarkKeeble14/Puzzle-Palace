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

    private InputMode currentInputMode;
    private bool hasDealtWIthPencilButton;

    public bool AllowMove { get; protected set; }
    [SerializeField] private float delayBetweenCellsInRestartSequence;
    [SerializeField] protected float delayOnRestart = 1.0f;

    [SerializeField] private List<int> allowedNums = new List<int>();
    protected override IEnumerator Setup()
    {
        // Generate the board
        board = Instantiate(sudokuBoardPrefab, parentSpawnedTo);

        yield return StartCoroutine(board.Generate(SelectCell, allowedNums, minMaxNumHoles));
    }

    protected override IEnumerator Restart()
    {
        AllowMove = false;
        yield return StartCoroutine(board.ActOnEachBoardCellWithDelay(cell =>
        {
            StartCoroutine(cell.ChangeScale(0));
            StartCoroutine(cell.ChangeTotalAlpha(0));
        }, delayBetweenCellsInRestartSequence, true));

        yield return new WaitForSeconds(delayOnRestart);

        Destroy(board.gameObject);

        AllowMove = true;
    }

    protected override IEnumerator GameWon()
    {
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
        if (selectedCell) selectedCell.Deselect();
        selectedCell = cell;
        selectedCell.Select();
    }

    private void ToggleInputMode()
    {
        switch (currentInputMode)
        {
            case InputMode.INPUT:
                currentInputMode = InputMode.PENCIL;
                pencilButton.SetColor(WordoDataDealer._Instance.GetActiveButtonColor());
                break;
            case InputMode.PENCIL:
                currentInputMode = InputMode.INPUT;
                pencilButton.SetColor(WordoDataDealer._Instance.GetInactiveButtonColor());
                break;
        }
    }

    protected override IEnumerator GameLoop()
    {
        if (!hasDealtWIthPencilButton)
        {
            hasDealtWIthPencilButton = true;
            pencilButton = virtualKeyboard.GetAdditionalFuncButton("PENCIL");
            pencilButton.SetColor(WordoDataDealer._Instance.GetInactiveButtonColor());
            additionalFunctionsDict.Add("PENCIL", ToggleInputMode);
        }

        string currentFrameString;
        while (true)
        {
            if (!selectedCell)
            {
                yield return null;
            }

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
                        switch (currentInputMode)
                        {
                            case InputMode.INPUT:
                                selectedCell.SetInputtedChar(x.ToString()[0]);
                                AudioManager._Instance.PlayFromSFXDict(onInput);

                                if (board.CheckForWin())
                                {
                                    yield break;
                                }

                                break;
                            case InputMode.PENCIL:
                                if (!selectedCell.GetInputtedChar().Equals(' ')) selectedCell.SetInputtedChar(' ');
                                selectedCell.TryPencilChar(x.ToString()[0]);
                                AudioManager._Instance.PlayFromSFXDict(onInput);
                                break;
                        }
                    }
                }
            }

            yield return null;
        }
    }
}
