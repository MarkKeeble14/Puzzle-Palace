using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class TicTacToeGameManager : MonoBehaviour
{
    public static TicTacToeGameManager _Instance { get; private set; }
    private bool gameStarted;

    private GameState gameState;
    [SerializeField] private TicTacToeBoard board;
    [SerializeField]
    private SerializableDictionary<TicTacToeBoardCellState, StateDisplayInfo> stateSpriteDict
        = new SerializableDictionary<TicTacToeBoardCellState, StateDisplayInfo>();
    private GameState winner;
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

    [System.Serializable]
    public struct StateDisplayInfo
    {
        public Sprite Sprite;
        public Color Color;
    }

    public enum TicTacToeBoardCellState
    {
        NULL,
        X,
        O
    }

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
        gameState = GameState.P1;

        StartCoroutine(StartSequence());
    }

    public void BeginGame()
    {
        gameStarted = true;
    }

    public void SetImageInfo(Image i, TicTacToeBoardCellState state)
    {
        i.sprite = stateSpriteDict[state].Sprite;
        i.color = stateSpriteDict[state].Color;
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
        yield return StartCoroutine(board.Generate());

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
                    board.SetUnsetBoardSpritesToSprite(stateSpriteDict[TicTacToeBoardCellState.O].Sprite);
                    yield return new WaitUntil(() => board.HasChanged);
                    if (board.GameWon)
                    {
                        StartCoroutine(board.StartWinCellsAnimation());
                        winner = gameState;
                        gameState = GameState.END;
                    }
                    else if (board.GameTied)
                    {
                        winner = GameState.END;
                        gameState = GameState.END;
                    }
                    else
                    {
                        board.ResetHasChanged();
                        gameState = GameState.P2;
                    }
                    break;
                case GameState.P2:
                    board.SetUnsetBoardSpritesToSprite(stateSpriteDict[TicTacToeBoardCellState.X].Sprite);
                    yield return new WaitUntil(() => board.HasChanged);
                    if (board.GameWon)
                    {
                        StartCoroutine(board.StartWinCellsAnimation());
                        winner = gameState;
                        gameState = GameState.END;
                    }
                    else if (board.GameTied)
                    {
                        winner = GameState.END;
                        gameState = GameState.END;
                    }
                    else
                    {
                        board.ResetHasChanged();
                        gameState = GameState.P1;
                    }
                    break;
                case GameState.END:
                    endGameScreen.SetActive(true);
                    if (winner == GameState.END)
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
                        if (winner == GameState.P1)
                        {
                            p1Score++;
                        }
                        else if (winner == GameState.P2)
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

    public TicTacToeBoardCellState GetCurrentPlayerSymbol()
    {
        if (gameState == GameState.P1)
        {
            return TicTacToeBoardCellState.O;
        }
        else if (gameState == GameState.P2)
        {
            return TicTacToeBoardCellState.X;
        }
        else
        {
            return TicTacToeBoardCellState.NULL;
        }
    }

    public StateDisplayInfo GetStateDisplayInfo(TicTacToeBoardCellState state)
    {
        return stateSpriteDict[state];
    }
}