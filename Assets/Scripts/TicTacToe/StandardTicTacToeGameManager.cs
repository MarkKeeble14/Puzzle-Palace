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

    protected override IEnumerator Restart()
    {
        yield return StartCoroutine(board.ActOnEachBoardCellWithDelay(cell =>
        {
            StartCoroutine(cell.ChangeScale(0));
            StartCoroutine(cell.ChangeTotalAlpha(0));
        }, delayBetweenCellsInRestartSequence, true));

        yield return new WaitForSeconds(delayOnRestart);

        Destroy(board.gameObject);

        // Reset the game state to player 1's turn
        SetTurn(TicTacToeGameState.P1);
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
            SetTurn(TicTacToeGameState.END);
        }
        else if (board.GameTied)
        {
            winner = WinnerOptions.NEITHER;
            SetTurn(TicTacToeGameState.END);
        }
        else
        {
            board.ResetHasChanged();
            SetTurn(TicTacToeGameState.P2);
        }
    }

    protected override IEnumerator HandleP2Turn()
    {
        board.SetUnsetBoardSpritesToSprite(GetStateDisplayInfo(TicTacToeBoardCellState.X).Sprite);
        yield return new WaitUntil(() => board.HasChanged);
        if (board.GameWon)
        {
            winner = WinnerOptions.P2;
            SetTurn(TicTacToeGameState.END);
        }
        else if (board.GameTied)
        {
            winner = WinnerOptions.NEITHER;
            SetTurn(TicTacToeGameState.END);
        }
        else
        {
            board.ResetHasChanged();
            SetTurn(TicTacToeGameState.P1);
        }
    }

    protected override IEnumerator CheckMoveResult(TicTacToeGameState moveOccurredOn, TicTacToeBoardCell alteredCell)
    {
        yield return StartCoroutine(board.CheckMoveResult(moveOccurredOn, alteredCell));
    }
}