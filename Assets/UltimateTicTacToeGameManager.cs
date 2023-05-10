using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class TempTicTacToeGameManager : MonoBehaviour
{
    public static TempTicTacToeGameManager _Instance { get; private set; }
    private bool gameStarted;
    private GameState gameState;
    private GameState winner;

    [Header("References")]
    [SerializeField] private Transform parentSpawnedTo;
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

    [SerializeField] private int numCells;

    private void Awake()
    {
        _Instance = this;
    }

    public void Restart()
    {
        StartCoroutine(RestartGame());
    }

    protected abstract IEnumerator RestartGame();

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

    public TicTacToeBoardCellStateVisualInfo GetStateDisplayInfo(TicTacToeBoardCellState state)
    {
        return TicTacToeDataDealer._Instance.GetStateDisplayInfo(state);
    }


    private void Start()
    {
        SetImageInfo(p1Image, TicTacToeBoardCellState.O);
        SetImageInfo(p2Image, TicTacToeBoardCellState.X);

        StartCoroutine(StartSequence());
    }

    private IEnumerator StartSequence()
    {
        yield return StartCoroutine(Setup());

        // Start the Game
        StartCoroutine(GameLoop());
    }

    protected abstract IEnumerator Setup();

    protected virtual IEnumerator HandleMenu()
    {
        yield return new WaitUntil(() => gameStarted);
        gameState = GameState.P1;
        menuScreen.SetActive(false);
    }

    protected virtual IEnumerator HandleEOG()
    {
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


    protected abstract IEnumerator HandleP1Turn();
    protected abstract IEnumerator HandleP2Turn();


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
                    yield return StartCoroutine(HandleMenu());
                    break;
                case GameState.P1:
                    yield return StartCoroutine(HandleP1Turn());
                    break;
                case GameState.P2:
                    yield return StartCoroutine(HandleP2Turn());
                    break;
                case GameState.END:
                    yield return StartCoroutine(HandleEOG());
                    yield break;
            }

        }
    }
}

public class UltimateTicTacToeGameManager : MonoBehaviour
{
    public static UltimateTicTacToeGameManager _Instance { get; private set; }
    private bool gameStarted;

    [SerializeField] private int numBoards;
    [SerializeField] private int numCells;

    private GameState gameState;
    [SerializeField] private Transform parentBoardTo;
    [SerializeField] private UltimateTicTacToeBoard boardPrefab;
    private UltimateTicTacToeBoard board;

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
        TicTacToeBoardCellStateVisualInfo info = GetStateDisplayInfo(state);
        i.sprite = info.Sprite;
        i.color = info.Color;
    }

    private void Awake()
    {
        _Instance = this;
    }

    private void Start()
    {
        SetImageInfo(p1Image, TicTacToeBoardCellState.O);
        SetImageInfo(p2Image, TicTacToeBoardCellState.X);

        StartCoroutine(StartSequence());
    }

    private IEnumerator StartSequence()
    {
        // Generate the board
        board = Instantiate(boardPrefab, parentBoardTo);

        yield return StartCoroutine(board.Generate(numBoards, numCells));

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
                    board.SetUnsetBoardSpritesToSprite(GetStateDisplayInfo(TicTacToeBoardCellState.X).Sprite);
                    yield return new WaitUntil(() => board.HasChanged);
                    if (board.GameWon)
                    {
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

    public TicTacToeBoardCellStateVisualInfo GetStateDisplayInfo(TicTacToeBoardCellState state)
    {
        return TicTacToeDataDealer._Instance.GetStateDisplayInfo(state);
    }
}
