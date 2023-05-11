using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class StandardTicTacToeGameManager : TicTacToeGameManager
{
    [SerializeField] private TicTacToeBoard boardPrefab;
    private TicTacToeBoard board;
    [SerializeField] private float delayBetweenCellsInRestartSequence;

    protected override IEnumerator RestartGame()
    {
        yield return StartCoroutine(board.ActOnEachBoardCellWithDelay(cell =>
        {
            StartCoroutine(cell.ChangeScale(0));
            StartCoroutine(cell.ChangeTotalAlpha(0));
        }, delayBetweenCellsInRestartSequence, true));

        yield return new WaitForSeconds(delayOnRestart);

        Destroy(board.gameObject);

        // Reset the game state to player 1's turn
        SetTurn(GameState.P1);

        StartCoroutine(StartSequence());
    }

    protected override IEnumerator Setup()
    {
        // Generate the board
        board = Instantiate(boardPrefab, parentSpawnedTo);

        yield return StartCoroutine(board.Generate(numCells, false));

        playButton.SetActive(true);
    }

    protected override IEnumerator HandleP1Turn()
    {
        board.SetUnsetBoardSpritesToSprite(GetStateDisplayInfo(TicTacToeBoardCellState.O).Sprite);
        yield return new WaitUntil(() => board.HasChanged);
        if (board.GameWon)
        {
            winner = WinnerOptions.P1;
            SetTurn(GameState.END);
        }
        else if (board.GameTied)
        {
            winner = WinnerOptions.NEITHER;
            SetTurn(GameState.END);
        }
        else
        {
            board.ResetHasChanged();
            SetTurn(GameState.P2);
        }
    }

    protected override IEnumerator HandleP2Turn()
    {
        board.SetUnsetBoardSpritesToSprite(GetStateDisplayInfo(TicTacToeBoardCellState.X).Sprite);
        yield return new WaitUntil(() => board.HasChanged);
        if (board.GameWon)
        {
            winner = WinnerOptions.P2;
            SetTurn(GameState.END);
        }
        else if (board.GameTied)
        {
            winner = WinnerOptions.NEITHER;
            SetTurn(GameState.END);
        }
        else
        {
            board.ResetHasChanged();
            SetTurn(GameState.P1);
        }
    }

    protected override IEnumerator CheckMoveResult(GameState moveOccurredOn, TicTacToeBoardCell alteredCell)
    {
        yield return StartCoroutine(board.CheckMoveResult(moveOccurredOn, alteredCell));
    }
}