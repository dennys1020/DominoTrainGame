using System;

namespace DominoTrainGame.Server.Application.Domain;

public sealed class LobbyState
{
    private const int EmptyMaximumPlayers = 0;
    private const int NoPlayers = 0;
    private const int MinimumPlayers = 2;

    static LobbyState()
    {
        Empty = new LobbyState(string.Empty, EmptyMaximumPlayers, LobbyRoster.Empty);
    }

    public LobbyState(string code, int maximumPlayers, LobbyRoster roster)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        MaximumPlayers = maximumPlayers;
        Roster = roster ?? throw new ArgumentNullException(nameof(roster));
    }

    public static LobbyState Empty { get; }

    public string Code { get; }

    public int MaximumPlayers { get; }

    public LobbyRoster Roster { get; }

    public bool IsFull => Roster.Count >= MaximumPlayers;

    public bool IsEmpty => Roster.Count == NoPlayers;

    public bool HasMinimumPlayers => Roster.Count >= MinimumPlayers;

    public static LobbyState CreateWithHost(string code, int maximumPlayers, LobbyMember host)
    {
        LobbyState emptyLobby = new LobbyState(code, maximumPlayers, LobbyRoster.Empty);
        LobbyState lobby = emptyLobby.WithPlayer(host);
        return lobby;
    }

    public LobbyState WithPlayer(LobbyMember member)
    {
        LobbyState lobby = new LobbyState(Code, MaximumPlayers, Roster.WithPlayer(member));
        return lobby;
    }

    public LobbyState WithReadyState(int playerId, bool isReady)
    {
        LobbyRoster roster = Roster.WithReadyState(playerId, isReady);
        LobbyState lobby = new LobbyState(Code, MaximumPlayers, roster);
        return lobby;
    }

    public LobbyState WithoutPlayer(int playerId)
    {
        LobbyState lobby = new LobbyState(Code, MaximumPlayers, Roster.WithoutPlayer(playerId));
        return lobby;
    }
}
