using System;
using System.Collections.Generic;
using System.Linq;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.State;

public sealed class LobbyRegistry
{
    private readonly ILobbyCodeGenerator _codeGenerator;
    private readonly Dictionary<string, LobbyState> _lobbiesByCode =
        new Dictionary<string, LobbyState>(StringComparer.Ordinal);
    private readonly object _lobbiesLock = new object();

    public LobbyRegistry(ILobbyCodeGenerator codeGenerator)
    {
        _codeGenerator = codeGenerator ?? throw new ArgumentNullException(nameof(codeGenerator));
    }

    public LobbyState Create(PlayerIdentity host)
    {
        if (host == null)
        {
            throw new ArgumentNullException(nameof(host));
        }

        LobbyState createdLobby = LobbyState.Empty;

        lock (_lobbiesLock)
        {
            if (FindLobby(host.PlayerId).IsEmpty)
            {
                createdLobby = AddLobby(host);
            }
        }

        return createdLobby;
    }

    public LobbyOutcome<JoinLobbyStatus> Join(PlayerIdentity member, string code)
    {
        if (member == null)
        {
            throw new ArgumentNullException(nameof(member));
        }

        string lobbyCode = code ?? string.Empty;
        LobbyOutcome<JoinLobbyStatus> outcome;

        lock (_lobbiesLock)
        {
            outcome = AddPlayer(member, lobbyCode);
        }

        return outcome;
    }

    public string NormalizeCode(string code)
    {
        string normalizedCode = code?.Trim().ToUpperInvariant() ?? string.Empty;

        if (!_codeGenerator.IsValid(normalizedCode))
        {
            throw new ArgumentException("The lobby code format is invalid.", nameof(code));
        }

        return normalizedCode;
    }

    public LobbyState RemovePlayer(int playerId)
    {
        LobbyState remainingLobby = UpdateLobbyOfPlayer(playerId, lobby => lobby.WithoutPlayer(playerId));
        return remainingLobby;
    }

    public LobbyState SetReady(int playerId, bool isReady)
    {
        LobbyState updatedLobby = UpdateLobbyOfPlayer(playerId, lobby => lobby.WithReadyState(playerId, isReady));
        return updatedLobby;
    }

    public LobbyState FindLobbyOfPlayer(int playerId)
    {
        LobbyState lobby;

        lock (_lobbiesLock)
        {
            lobby = FindLobby(playerId);
        }

        return lobby;
    }

    private LobbyState UpdateLobbyOfPlayer(int playerId, Func<LobbyState, LobbyState> update)
    {
        LobbyState updatedLobby = LobbyState.Empty;

        lock (_lobbiesLock)
        {
            LobbyState currentLobby = FindLobby(playerId);

            if (!currentLobby.IsEmpty)
            {
                updatedLobby = update(currentLobby);
                StoreLobby(updatedLobby);
            }
        }

        return updatedLobby;
    }

    private LobbyState AddLobby(PlayerIdentity host)
    {
        string code = GenerateUniqueCode();
        LobbyState createdLobby = LobbyState.CreateWithHost(code, LobbyState.MaximumPlayerCount, host);

        _lobbiesByCode.Add(code, createdLobby);
        return createdLobby;
    }

    private LobbyOutcome<JoinLobbyStatus> AddPlayer(PlayerIdentity member, string code)
    {
        LobbyState currentLobby = FindLobbyByCode(code);
        LobbyState joinedLobby = LobbyState.Empty;
        JoinLobbyStatus status = JoinLobbyStatus.Success;

        if (!FindLobby(member.PlayerId).IsEmpty)
        {
            status = JoinLobbyStatus.AlreadyInLobby;
        }
        else if (currentLobby.IsEmpty)
        {
            status = JoinLobbyStatus.LobbyNotFound;
        }
        else if (currentLobby.IsFull)
        {
            status = JoinLobbyStatus.LobbyFull;
        }
        else
        {
            joinedLobby = currentLobby.WithPlayer(member);
            _lobbiesByCode[code] = joinedLobby;
        }

        LobbyOutcome<JoinLobbyStatus> outcome = new LobbyOutcome<JoinLobbyStatus>(status, joinedLobby);
        return outcome;
    }

    private void StoreLobby(LobbyState lobby)
    {
        if (lobby.HasPlayers)
        {
            _lobbiesByCode[lobby.Code] = lobby;
        }
        else
        {
            _lobbiesByCode.Remove(lobby.Code);
        }
    }

    private string GenerateUniqueCode()
    {
        string code = _codeGenerator.Generate();

        while (_lobbiesByCode.ContainsKey(code))
        {
            code = _codeGenerator.Generate();
        }

        return code;
    }

    private LobbyState FindLobbyByCode(string code)
    {
        LobbyState lobby = _lobbiesByCode.ContainsKey(code) ? _lobbiesByCode[code] : LobbyState.Empty;
        return lobby;
    }

    private LobbyState FindLobby(int playerId)
    {
        LobbyState lobby = _lobbiesByCode.Values.FirstOrDefault(candidate => candidate.Roster.Contains(playerId))
            ?? LobbyState.Empty;
        return lobby;
    }
}
