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
        SetTurn(defaultTurn);
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
