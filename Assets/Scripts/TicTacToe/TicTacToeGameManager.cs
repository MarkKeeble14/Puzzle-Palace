using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class TicTacToeGameManager : MiniGameManager
{
    protected TwoPlayerGameState gameState;
    protected WinnerOptions winner;

    [Header("References")]
    [SerializeField] private ScreenAnimationController eogScreenAnimationHelper;
    [SerializeField] private ScreenAnimationController beginGameScreenAnimationHelper;
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
    [SerializeField] protected int numCells;

    [SerializeField] protected TwoPlayerGameState defaultTurn;

    public bool AllowMove { get; protected set; }

    protected TicTacToeBoardCellStateVisualInfo GetStateDisplayInfo(TwoPlayerCellState state)
    {
        return TwoPlayerDataDealer._Instance.GetStateVisualInfo(state);
    }

    public void SetDefaultTurn(int state)
    {
        switch (state)
        {
            case 0:
                defaultTurn = TwoPlayerGameState.P1;
                break;
            case 1:
                defaultTurn = TwoPlayerGameState.P2;
                break;
            default:
                throw new System.Exception("Invalid Integer Parameter Passed Through to Set Turn");
        }
    }

    protected virtual IEnumerator HandleMenu()
    {
        yield return new WaitUntil(() => gameStarted);
        SetTurn(defaultTurn);
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

    protected abstract IEnumerator HandleP1Turn();
    protected abstract IEnumerator HandleP2Turn();

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

    protected abstract IEnumerator CheckMoveResult(TwoPlayerGameState moveOccurredOn, TicTacToeBoardCell alteredCell);

    public IEnumerator NotifyOfMove(TicTacToeBoardCell alteredCell)
    {
        AllowMove = false;
        yield return CheckMoveResult(gameState, alteredCell);
    }

    protected Sprite GetCurrentPlayerSymbol()
    {
        return TwoPlayerDataDealer._Instance.GetCurrentPlayerInfo().VisualInfo.Sprite;
    }
}
