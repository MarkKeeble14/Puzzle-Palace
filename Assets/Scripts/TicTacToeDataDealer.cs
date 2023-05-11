using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Player
{
    PLAYER1,
    PLAYER2
}

[System.Serializable]
public struct PlayerRepresentationInformation
{
    public TicTacToeBoardCellState CellState;
    public TicTacToeBoardCellStateVisualInfo VisualInfo;
}

public class TicTacToeDataDealer : MonoBehaviour
{
    public static TicTacToeDataDealer _Instance { get; private set; }

    [SerializeField]
    private SerializableDictionary<Player, PlayerRepresentationInformation> playerRepresentingDict = new SerializableDictionary<Player, PlayerRepresentationInformation>();
    private Dictionary<GameState, Player> playerGameStateLinker = new Dictionary<GameState, Player>();
    private Dictionary<TicTacToeBoardCellState, Player> playerCellStateLinker = new Dictionary<TicTacToeBoardCellState, Player>();
    private Dictionary<WinnerOptions, Player> winnerOptionsPlayerLinker = new Dictionary<WinnerOptions, Player>();

    [SerializeField] private TicTacToeBoardCellStateVisualInfo nullCellVisualInfo;
    [SerializeField] private Color winCellColor;
    [SerializeField] private Color uninteractableCellColor;
    [SerializeField] private Color interactableCellColor;

    public GameState CurrentTurn { get; set; }

    [SerializeField] private Image p1Image;
    [SerializeField] private Image p2Image;

    private void Awake()
    {
        _Instance = this;

        playerGameStateLinker.Add(GameState.P1, Player.PLAYER1);
        playerGameStateLinker.Add(GameState.P2, Player.PLAYER2);

        playerCellStateLinker.Add(playerRepresentingDict[Player.PLAYER1].CellState, Player.PLAYER1);
        playerCellStateLinker.Add(playerRepresentingDict[Player.PLAYER2].CellState, Player.PLAYER2);

        winnerOptionsPlayerLinker.Add(WinnerOptions.P1, Player.PLAYER1);
        winnerOptionsPlayerLinker.Add(WinnerOptions.P2, Player.PLAYER2);

        SetImageInfo(p1Image, Player.PLAYER1);
        SetImageInfo(p2Image, Player.PLAYER2);
    }

    public void SetImageInfo(Image i, Player player)
    {
        TicTacToeBoardCellStateVisualInfo info = playerRepresentingDict[player].VisualInfo;
        i.sprite = info.Sprite;
        i.color = info.Color;
    }

    public PlayerRepresentationInformation GetCurrentPlayerInfo()
    {
        return playerRepresentingDict[playerGameStateLinker[CurrentTurn]];
    }

    public Color GetPlayerColor(WinnerOptions player)
    {
        return playerRepresentingDict[winnerOptionsPlayerLinker[player]].VisualInfo.Color;
    }

    public Color GetPlayerColor(GameState gameState)
    {
        return playerRepresentingDict[playerGameStateLinker[gameState]].VisualInfo.Color;
    }


    public TicTacToeBoardCellStateVisualInfo GetStateVisualInfo(TicTacToeBoardCellState state)
    {
        if (state == TicTacToeBoardCellState.NULL)
        {
            return nullCellVisualInfo;
        }
        return playerRepresentingDict[playerCellStateLinker[state]].VisualInfo;
    }

    public TicTacToeBoardCellState GetCurrentPlayerSymbol()
    {
        return playerRepresentingDict[playerGameStateLinker[CurrentTurn]].CellState;
    }

    public Color GetWinCellColor()
    {
        return winCellColor;
    }

    public Color GetCellColor(bool interactable)
    {
        return interactable ? interactableCellColor : uninteractableCellColor;
    }
}
