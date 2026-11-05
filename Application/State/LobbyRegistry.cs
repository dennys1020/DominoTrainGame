using System;
using System.Collections.Generic;
using System.Linq;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.State;

public sealed class LobbyRegistry
{
    private const int MaximumPlayers = 4;

    private readonly ILobbyCodeGenerator _codeGenerator;
    private readonly Dictionary<string, LobbyState> _lobbiesByCode =
        new Dictionary<string, LobbyState>(StringComparer.Ordinal);
    private readonly object _lobbiesLock = new object();

    public LobbyRegistry(ILobbyCodeGenerator codeGenerator)
    {
        _codeGenerator = codeGenerator ?? throw new ArgumentNullException(nameof(codeGenerator));
    }

    public bool TryCreate(LobbyMember host, out LobbyState lobbyState)
    {
        if (host == null)
        {
            throw new ArgumentNullException(nameof(host));
        }

        lobbyState = LobbyState.Empty;
        bool wasCreated = false;

        lock (_lobbiesLock)
        {
            if (!ContainsPlayer(host.PlayerId))
            {
                lobbyState = AddLobby(host);
                wasCreated = true;
            }
        }

        return wasCreated;
    }

    public JoinLobbyStatus TryJoin(LobbyMember member, string code, out LobbyState lobbyState)
    {
        if (member == null)
        {
            throw new ArgumentNullException(nameof(member));
        }

        string normalizedCode = code?.Trim().ToUpperInvariant() ?? string.Empty;
        JoinLobbyStatus status;

        lock (_lobbiesLock)
        {
            status = AddPlayer(member, normalizedCode, out lobbyState);
        }

        return status;
    }

    public bool Remove(int playerId, out LobbyState remainingLobby)
    {
        bool wasRemoved = TryUpdateLobbyOfPlayer(
            playerId,
            lobby => lobby.WithoutPlayer(playerId),
            out remainingLobby);
        return wasRemoved;
    }

    public bool TrySetReady(int playerId, bool isReady, out LobbyState lobbyState)
    {
        bool wasUpdated = TryUpdateLobbyOfPlayer(
            playerId,
            lobby => lobby.WithReadyState(playerId, isReady),
            out lobbyState);
        return wasUpdated;
    }

    public bool TryFindLobbyOfPlayer(int playerId, out LobbyState lobbyState)
    {
        bool wasFound;

        lock (_lobbiesLock)
        {
            wasFound = TryFindLobby(playerId, out lobbyState);
        }

        return wasFound;
    }

    private bool TryUpdateLobbyOfPlayer(
        int playerId,
        Func<LobbyState, LobbyState> update,
        out LobbyState lobbyState)
    {
        lobbyState = LobbyState.Empty;
        bool wasFound;

        lock (_lobbiesLock)
        {
            wasFound = TryFindLobby(playerId, out LobbyState currentLobby);

            if (wasFound)
            {
                lobbyState = update(currentLobby);
                StoreLobby(lobbyState);
            }
        }

        return wasFound;
    }

    private LobbyState AddLobby(LobbyMember host)
    {
        string code = GenerateUniqueCode();
        LobbyState createdLobby = LobbyState.CreateWithHost(code, MaximumPlayers, host);

        _lobbiesByCode.Add(code, createdLobby);
        return createdLobby;
    }

    private JoinLobbyStatus AddPlayer(LobbyMember member, string code, out LobbyState lobbyState)
    {
        lobbyState = LobbyState.Empty;
        JoinLobbyStatus status = JoinLobbyStatus.Success;
        bool wasFound = _lobbiesByCode.TryGetValue(code, out LobbyState currentLobby);

        if (ContainsPlayer(member.PlayerId))
        {
            status = JoinLobbyStatus.AlreadyInLobby;
        }
        else if (!wasFound)
        {
            status = JoinLobbyStatus.LobbyNotFound;
        }
        else if (currentLobby.IsFull)
        {
            status = JoinLobbyStatus.LobbyFull;
        }
        else
        {
            lobbyState = currentLobby.WithPlayer(member);
            _lobbiesByCode[code] = lobbyState;
        }

        return status;
    }

    private void StoreLobby(LobbyState lobby)
    {
        if (lobby.IsEmpty)
        {
            _lobbiesByCode.Remove(lobby.Code);
        }
        else
        {
            _lobbiesByCode[lobby.Code] = lobby;
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

    private bool ContainsPlayer(int playerId)
    {
        bool isPresent = _lobbiesByCode.Values.Any(lobby => lobby.Roster.Contains(playerId));
        return isPresent;
    }

    private bool TryFindLobby(int playerId, out LobbyState lobby)
    {
        lobby = LobbyState.Empty;
        bool wasFound = false;

        foreach (LobbyState candidate in _lobbiesByCode.Values)
        {
            if (!wasFound && candidate.Roster.Contains(playerId))
            {
                lobby = candidate;
                wasFound = true;
            }
        }

        return wasFound;
    }
}
