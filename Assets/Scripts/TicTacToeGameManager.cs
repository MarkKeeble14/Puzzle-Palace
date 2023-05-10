using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class TicTacToeGameManager : MonoBehaviour
{
    public static TicTacToeGameManager _Instance { get; private set; }
    private bool gameStarted;

    [SerializeField] private int boardSize;

    private GameState gameState;
    [SerializeField] private Transform parentBoardTo;
    [SerializeField] private TicTacToeBoard boardPrefab;
    private TicTacToeBoard board;
    private WinnerOptions winner;
    [SerializeField] private float delayBetweenCellsInRestartSequence;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private Animator turnTextAnimator;
    [SerializeField] private Image p1Image;
    [SerializeField] private Image p2Image;
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private string winnerTextString = "Winner: ";
    [SerializeField] private string turnTextString = "'s Move";
    [SerializeField] private GameObject menuScreen;
    [SerializeField] private GameObject endGameScreen;
    [SerializeField] private TextMeshProUGUI p1ScoreText;
    [SerializeField] private TextMeshProUGUI p2ScoreText;
    [SerializeField] private GameObject playButton;

    private int p1Score;
    private int p2Score;

    public void Restart()
    {
        StartCoroutine(RestartGame());
    }

    private IEnumerator RestartGame()
    {
        endGameScreen.SetActive(false);

        yield return StartCoroutine(board.ActOnEachBoardCellWithDelay(cell =>
        {
            cell.SetAnimatorParameterBool("Clear", true);
        }, delayBetweenCellsInRestartSequence, true));

        yield return new WaitForSeconds(1.0f);

        // Reset the game state to player 1's turn
        SetTurn(GameState.P1);

        StartCoroutine(StartSequence());
    }

    private void SetTurn(GameState state)
    {
        if (state == GameState.P1 || state == GameState.P2)
        {
            TicTacToeDataDealer._Instance.CurrentTurn = state;
        }
        gameState = state;
    }

    public void BeginGame()
    {
        gameStarted = true;
    }

    public void SetImageInfo(Image i, TicTacToeBoardCellState state)
    {
        TicTacToeBoardCellStateVisualInfo info = GetStateDisplayInfo(state);
        i.sprite = info.Sprite;
        i.color = info.Color;
    }

    private void Awake()
    {
        _Instance = this;

        SetImageInfo(p1Image, TicTacToeBoardCellState.O);
        SetImageInfo(p2Image, TicTacToeBoardCellState.X);

        StartCoroutine(StartSequence());
    }

    private IEnumerator StartSequence()
    {
        // Generate the board
        board = Instantiate(boardPrefab, parentBoardTo);

        yield return StartCoroutine(board.Generate(boardSize));

        if (menuScreen.activeInHierarchy)
            playButton.SetActive(true);

        // Start the Game
        StartCoroutine(GameLoop());
    }

    public void NotifyOfMove(TicTacToeBoardCell alteredCell)
    {
        board.NotifyOfMove(alteredCell);
    }

    private IEnumerator GameLoop()
    {
        while (true)
        {
            // Set turn text
            if (gameState == GameState.P1 || gameState == GameState.P2)
            {
                turnText.text = gameState.ToString() + turnTextString;
                turnTextAnimator.SetBool("In", true);
            }
            else
            {
                turnTextAnimator.SetBool("In", false);
                turnText.text = "";
            }

            // Act depending on turn
            switch (gameState)
            {
                case GameState.MENU:
                    yield return new WaitUntil(() => gameStarted);
                    gameState = GameState.P1;
                    menuScreen.SetActive(false);
                    break;
                case GameState.P1:
                    board.SetUnsetBoardSpritesToSprite(GetStateDisplayInfo(TicTacToeBoardCellState.O).Sprite);
                    yield return new WaitUntil(() => board.HasChanged);
                    if (board.GameWon)
                    {
                        StartCoroutine(board.StartWinCellsAnimation());
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
                    break;
                case GameState.P2:
                    board.SetUnsetBoardSpritesToSprite(GetStateDisplayInfo(TicTacToeBoardCellState.X).Sprite);
                    yield return new WaitUntil(() => board.HasChanged);
                    if (board.GameWon)
                    {
                        StartCoroutine(board.StartWinCellsAnimation());
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
                    break;
                case GameState.END:
                    endGameScreen.SetActive(true);
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
        }
    }

    private TicTacToeBoardCellStateVisualInfo GetStateDisplayInfo(TicTacToeBoardCellState state)
    {
        return TicTacToeDataDealer._Instance.GetStateDisplayInfo(state);
    }
}