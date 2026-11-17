using System;

namespace DominoTrainGame.Server.Application.Domain;

public sealed class LobbyState
{
    private const int NoPlayers = 0;

    static LobbyState()
    {
        Empty = new LobbyState(string.Empty, LobbyRoster.Empty, DominoSet.DoubleTwelve);
    }

    public LobbyState(string code, LobbyRoster roster, DominoSet dominoSet)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Roster = roster ?? throw new ArgumentNullException(nameof(roster));
        DominoSet = dominoSet ?? throw new ArgumentNullException(nameof(dominoSet));
    }

    public static LobbyState Empty { get; }

    public string Code { get; }

    public LobbyRoster Roster { get; }

    public DominoSet DominoSet { get; }

    public int MaximumPlayers => DominoSet.MaximumPlayerCount;

    public bool IsFull => Roster.Count >= MaximumPlayers;

    public bool IsEmpty => ReferenceEquals(this, Empty);

    public bool HasPlayers => Roster.Count > NoPlayers;

    public bool HasMinimumPlayers => Roster.Count >= DominoSet.MinimumPlayerCount;

    public static LobbyState CreateWithHost(string code, PlayerIdentity host)
    {
        LobbyState emptyLobby = new LobbyState(code, LobbyRoster.Empty, DominoSet.DoubleTwelve);
        LobbyState lobby = emptyLobby.WithPlayer(host);
        return lobby;
    }

    public LobbyState WithPlayer(PlayerIdentity member)
    {
        LobbyState lobby = new LobbyState(Code, Roster.WithPlayer(member), DominoSet);
        return lobby;
    }

    public LobbyState WithReadyState(int playerId, bool isReady)
    {
        LobbyRoster roster = Roster.WithReadyState(playerId, isReady);
        LobbyState lobby = new LobbyState(Code, roster, DominoSet);
        return lobby;
    }

    public LobbyState WithoutPlayer(int playerId)
    {
        LobbyState lobby = new LobbyState(Code, Roster.WithoutPlayer(playerId), DominoSet);
        return lobby;
    }

    public LobbyState WithDominoSet(DominoSet dominoSet)
    {
        LobbyState lobby = new LobbyState(Code, Roster.WithNextVersion(), dominoSet);
        return lobby;
    }

    public bool FitsDominoSet(DominoSet dominoSet)
    {
        if (dominoSet == null)
        {
            throw new ArgumentNullException(nameof(dominoSet));
        }

        bool fitsDominoSet = Roster.Count <= dominoSet.MaximumPlayerCount;
        return fitsDominoSet;
    }
}
