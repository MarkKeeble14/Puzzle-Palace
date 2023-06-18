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
            cell.SetInteractable(false);
            StartCoroutine(cell.ChangeScale(0));
        }, delayBetweenCellsInRestartSequence, true));

        yield return new WaitForSeconds(delayOnRestart);

        Destroy(board.gameObject);

        // Reset the game state to player 1's turn
        SetTurn(TwoPlayerGameState.P1);
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
        board.SetUnsetBoardSpritesToSprite(GetStateDisplayInfo(TwoPlayerCellState.O).Sprite);
        yield return new WaitUntil(() => board.HasChanged);
        if (board.GameWon)
        {
            winner = WinnerOptions.P1;
            SetTurn(TwoPlayerGameState.END);
        }
        else if (board.GameTied)
        {
            winner = WinnerOptions.NEITHER;
            SetTurn(TwoPlayerGameState.END);
        }
        else
        {
            board.ResetHasChanged();
            SetTurn(TwoPlayerGameState.P2);
        }
    }

    protected override IEnumerator HandleP2Turn()
    {
        board.SetUnsetBoardSpritesToSprite(GetStateDisplayInfo(TwoPlayerCellState.X).Sprite);
        yield return new WaitUntil(() => board.HasChanged);
        if (board.GameWon)
        {
            winner = WinnerOptions.P2;
            SetTurn(TwoPlayerGameState.END);
        }
        else if (board.GameTied)
        {
            winner = WinnerOptions.NEITHER;
            SetTurn(TwoPlayerGameState.END);
        }
        else
        {
            board.ResetHasChanged();
            SetTurn(TwoPlayerGameState.P1);
        }
    }

    protected override IEnumerator CheckMoveResult(TwoPlayerGameState moveOccurredOn, TicTacToeBoardCell alteredCell)
    {
        yield return StartCoroutine(board.CheckMoveResult(moveOccurredOn, alteredCell));
    }
}