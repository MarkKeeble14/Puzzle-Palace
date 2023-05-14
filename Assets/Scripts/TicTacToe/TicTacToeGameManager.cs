using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class TicTacToeGameManager : MiniGameManager
{
    protected TicTacToeGameState gameState;
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

    public bool AllowMove { get; protected set; }

    public void SetImageInfo(Image i, TicTacToeBoardCellState state)
    {
        TicTacToeBoardCellStateVisualInfo info = GetStateDisplayInfo(state);
        i.sprite = info.Sprite;
        i.color = info.Color;
    }

    public TicTacToeBoardCellStateVisualInfo GetStateDisplayInfo(TicTacToeBoardCellState state)
    {
        return TicTacToeDataDealer._Instance.GetStateVisualInfo(state);
    }

    protected virtual IEnumerator HandleMenu()
    {
        yield return new WaitUntil(() => gameStarted);
        SetTurn(TicTacToeGameState.P1);
        beginGameScreenAnimationHelper.Fade(true);
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
            if (gameState == TicTacToeGameState.P1 || gameState == TicTacToeGameState.P2)
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
                case TicTacToeGameState.MENU:
                    yield return StartCoroutine(HandleMenu());
                    break;
                case TicTacToeGameState.P1:
                    AllowMove = true;
                    yield return StartCoroutine(HandleP1Turn());
                    break;
                case TicTacToeGameState.P2:
                    AllowMove = true;
                    yield return StartCoroutine(HandleP2Turn());
                    break;
                case TicTacToeGameState.END:
                    yield break;
            }

        }
    }

    protected void SetTurn(TicTacToeGameState state)
    {
        if (state == TicTacToeGameState.P1 || state == TicTacToeGameState.P2)
        {
            TicTacToeDataDealer._Instance.CurrentTurn = state;
        }
        gameState = state;
    }

    protected abstract IEnumerator CheckMoveResult(TicTacToeGameState moveOccurredOn, TicTacToeBoardCell alteredCell);

    public IEnumerator NotifyOfMove(TicTacToeBoardCell alteredCell)
    {
        AllowMove = false;
        yield return CheckMoveResult(gameState, alteredCell);
        AllowMove = true;
    }

    protected Sprite GetCurrentPlayerSymbol()
    {
        return TicTacToeDataDealer._Instance.GetCurrentPlayerInfo().VisualInfo.Sprite;
    }
}
