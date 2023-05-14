using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltimateTicTacToeGameManager : TicTacToeGameManager
{
    [SerializeField] private int numBoards;
    [SerializeField] private UltimateTicTacToeBoard boardPrefab;
    private UltimateTicTacToeBoard board;

    protected override IEnumerator Restart()
    {
        board.ActOnEachBoard(board =>
        {
            StartCoroutine(board.ChangeScale(0));
            StartCoroutine(board.ChangeTotalAlpha(0));
        });

        yield return new WaitForSeconds(delayOnRestart);

        Destroy(board.gameObject);

        // Reset the game state to player 1's turn
        SetTurn(TicTacToeGameState.P1);
    }

    protected override IEnumerator Setup()
    {
        // Generate the board
        board = Instantiate(boardPrefab, parentSpawnedTo);

        yield return StartCoroutine(board.Generate(numBoards, numCells));

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
