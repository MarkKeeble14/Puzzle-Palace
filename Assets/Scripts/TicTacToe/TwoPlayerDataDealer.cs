using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TwoPlayerDataDealer : CellDataDealer
{
    public new static TwoPlayerDataDealer _Instance { get; private set; }

    [SerializeField]
    private SerializableDictionary<Player, PlayerRepresentationInformation> playerRepresentingDict = new SerializableDictionary<Player, PlayerRepresentationInformation>();
    private Dictionary<TwoPlayerGameState, Player> playerGameStateLinker = new Dictionary<TwoPlayerGameState, Player>();
    private Dictionary<TwoPlayerCellState, Player> playerCellStateLinker = new Dictionary<TwoPlayerCellState, Player>();
    private Dictionary<WinnerOptions, Player> winnerOptionsPlayerLinker = new Dictionary<WinnerOptions, Player>();

    public TwoPlayerGameState CurrentTurn { get; set; }

    [SerializeField] private Image p1Image;
    [SerializeField] private Image p2Image;

    protected override void Awake()
    {
        _Instance = this;

        playerGameStateLinker.Add(TwoPlayerGameState.P1, Player.PLAYER1);
        playerGameStateLinker.Add(TwoPlayerGameState.P2, Player.PLAYER2);

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

    public Color GetPlayerColor(TwoPlayerGameState gameState)
    {
        return playerRepresentingDict[playerGameStateLinker[gameState]].VisualInfo.Color;
    }

    public TicTacToeBoardCellStateVisualInfo GetStateVisualInfo(TwoPlayerCellState state)
    {
        if (state == TwoPlayerCellState.NULL)
        {
            return nullCellVisualInfo;
        }
        return playerRepresentingDict[playerCellStateLinker[state]].VisualInfo;
    }

    public TwoPlayerCellState GetCurrentPlayerSymbol()
    {
        return playerRepresentingDict[playerGameStateLinker[CurrentTurn]].CellState;
    }
}
