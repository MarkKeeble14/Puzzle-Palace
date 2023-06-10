using System.Collections;
using TMPro;
using UnityEngine;

public class ConnectFourGameManager : MiniGameManager
{
    [SerializeField] private ConnectFourBoard boardPrefab;
    private ConnectFourBoard board;

    protected TwoPlayerGameState gameState;
    protected WinnerOptions winner;

    [Header("References")]
    [SerializeField] protected Transform parentSpawnedTo;
    [SerializeField] protected TextMeshProUGUI turnText;
    [SerializeField] protected Animator turnTextAnimator;
    [SerializeField] protected TextMeshProUGUI winnerText;
    [SerializeField] protected string winnerTextString = "Winner: ";
    [SerializeField] protected string turnTextString = "'s Move";
    [SerializeField] protected TextMeshProUGUI p1ScoreText;
    [SerializeField] protected TextMeshProUGUI p2ScoreText;
    [SerializeField] protected GameObject playButton;
    protected int p1Score;
    protected int p2Score;

    [SerializeField] protected float delayOnRestart = 1.0f;
    [SerializeField] protected Vector2Int gridSize;

    [SerializeField] private float delayBetweenCellsInRestartSequence;

    public bool AllowMove { get; protected set; }

    protected TicTacToeBoardCellStateVisualInfo GetStateDisplayInfo(TwoPlayerCellState state)
    {
        return TwoPlayerDataDealer._Instance.GetStateVisualInfo(state);
    }

    protected virtual IEnumerator HandleMenu()
    {
        yield return new WaitUntil(() => gameStarted);
        SetTurn(TwoPlayerGameState.P1);
        // beginGameScreenAnimationHelper.Fade(0.0f);
    }

    protected override IEnumerator GameWon()
    {
        if (winner == WinnerOptions.NEITHER)
        {
            // Tie Game
            winnerText.text = "Tie Game!";
            p1ScoreText.text = p1Score.ToString();
            p2ScoreText.text = p2Score.ToString();
        }
        else
        {
            // A player won
            winnerText.text = winnerTextString + winner.ToString();
            if (winner == WinnerOptions.P1)
            {
                p1Score++;
            }
            else if (winner == WinnerOptions.P2)
            {
                p2Score++;
            }
            p1ScoreText.text = p1Score.ToString();
            p2ScoreText.text = p2Score.ToString();
        }
        yield break;
    }

    protected override IEnumerator GameLoop()
    {
        while (true)
        {
            // Set turn text
            if (gameState == TwoPlayerGameState.P1 || gameState == TwoPlayerGameState.P2)
            {
                turnText.text = gameState.ToString() + turnTextString;
                turnTextAnimator.SetBool("In", true);
            }
            else
            {
                turnTextAnimator.SetBool("In", false);
            }

            // Act depending on turn
            switch (gameState)
            {
                case TwoPlayerGameState.MENU:
                    yield return StartCoroutine(HandleMenu());
                    break;
                case TwoPlayerGameState.P1:
                    AllowMove = true;
                    yield return StartCoroutine(HandleP1Turn());
                    break;
                case TwoPlayerGameState.P2:
                    AllowMove = true;
                    yield return StartCoroutine(HandleP2Turn());
                    break;
                case TwoPlayerGameState.END:
                    yield break;
            }

        }
    }

    protected void SetTurn(TwoPlayerGameState state)
    {
        if (state == TwoPlayerGameState.P1 || state == TwoPlayerGameState.P2)
        {
            TwoPlayerDataDealer._Instance.CurrentTurn = state;
        }
        gameState = state;
    }

    public IEnumerator NotifyOfMove(ConnectFourBoardCell alteredCell)
    {
        AllowMove = false;

        yield return StartCoroutine(board.ShowMove(alteredCell, TwoPlayerDataDealer._Instance.GetCurrentPlayerInfo().CellState));

        yield return CheckMoveResult(gameState, alteredCell);

        AllowMove = true;
    }

    protected Sprite GetCurrentPlayerSymbol()
    {
        return TwoPlayerDataDealer._Instance.GetCurrentPlayerInfo().VisualInfo.Sprite;
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

        // Reset the game state to player 1's turn
        SetTurn(TwoPlayerGameState.P1);

        AllowMove = true;
    }

    protected override IEnumerator Setup()
    {
        // Generate the board
        board = Instantiate(boardPrefab, parentSpawnedTo);

        yield return StartCoroutine(board.Generate(gridSize));

        playButton.SetActive(true);
    }

    protected IEnumerator HandleP1Turn()
    {
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

    protected IEnumerator HandleP2Turn()
    {
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

    protected IEnumerator CheckMoveResult(TwoPlayerGameState moveOccurredOn, ConnectFourBoardCell alteredCell)
    {
        yield return StartCoroutine(board.CheckMoveResult(moveOccurredOn, alteredCell));
    }
}