using System;
using System.Collections.Generic;
using System.Threading;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.Connection;

public sealed class LobbyRegistry
{
    private const int MaximumPlayers = 4;

    private readonly ILobbyCodeGenerator _codeGenerator;
    private RegistrySnapshot _snapshot = RegistrySnapshot.Empty;

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
        bool operationCompleted = false;

        while (!operationCompleted)
        {
            RegistrySnapshot current = Volatile.Read(ref _snapshot);

            if (ContainsPlayer(current, host.PlayerId))
            {
                operationCompleted = true;
            }
            else
            {
                string code = GenerateUniqueCode(current);
                LobbyState createdLobby = LobbyState.CreateWithHost(code, MaximumPlayers, host);
                Dictionary<string, LobbyState> updatedLobbies = CopyLobbies(current);

                updatedLobbies.Add(code, createdLobby);

                if (TryReplaceSnapshot(current, updatedLobbies))
                {
                    lobbyState = createdLobby;
                    wasCreated = true;
                    operationCompleted = true;
                }
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

        lobbyState = LobbyState.Empty;
        JoinLobbyStatus status = JoinLobbyStatus.Success;
        bool operationCompleted = false;

        while (!operationCompleted)
        {
            RegistrySnapshot current = Volatile.Read(ref _snapshot);

            if (ContainsPlayer(current, member.PlayerId))
            {
                status = JoinLobbyStatus.AlreadyInLobby;
                operationCompleted = true;
            }
            else if (!current.LobbiesByCode.TryGetValue(normalizedCode, out LobbyState currentLobby))
            {
                status = JoinLobbyStatus.LobbyNotFound;
                operationCompleted = true;
            }
            else if (currentLobby.IsFull)
            {
                status = JoinLobbyStatus.LobbyFull;
                operationCompleted = true;
            }
            else
            {
                LobbyState updatedLobby = currentLobby.WithPlayer(member);
                Dictionary<string, LobbyState> updatedLobbies = CopyLobbies(current);

                updatedLobbies[normalizedCode] = updatedLobby;

                if (TryReplaceSnapshot(current, updatedLobbies))
                {
                    lobbyState = updatedLobby;
                    operationCompleted = true;
                }
            }
        }

        return status;
    }

    public bool Remove(int playerId, out LobbyState remainingLobby)
    {
        remainingLobby = LobbyState.Empty;
        bool wasRemoved = false;
        bool operationCompleted = false;

        while (!operationCompleted)
        {
            RegistrySnapshot current = Volatile.Read(ref _snapshot);
            bool wasFound = TryFindLobby(current, playerId, out LobbyState currentLobby);

            if (!wasFound)
            {
                operationCompleted = true;
            }
            else
            {
                LobbyState updatedLobby = currentLobby.WithoutPlayer(playerId);
                Dictionary<string, LobbyState> updatedLobbies = CopyLobbies(current);

                if (updatedLobby.IsEmpty)
                {
                    updatedLobbies.Remove(currentLobby.Code);
                }
                else
                {
                    updatedLobbies[currentLobby.Code] = updatedLobby;
                }

                if (TryReplaceSnapshot(current, updatedLobbies))
                {
                    remainingLobby = updatedLobby;
                    wasRemoved = true;
                    operationCompleted = true;
                }
            }
        }

        return wasRemoved;
    }

    public bool TrySetReady(int playerId, bool isReady, out LobbyState lobbyState)
    {
        lobbyState = LobbyState.Empty;
        bool wasUpdated = false;
        bool operationCompleted = false;

        while (!operationCompleted)
        {
            RegistrySnapshot current = Volatile.Read(ref _snapshot);
            bool wasFound = TryFindLobby(current, playerId, out LobbyState currentLobby);

            if (!wasFound)
            {
                operationCompleted = true;
            }
            else
            {
                LobbyState updatedLobby = currentLobby.WithReadyState(playerId, isReady);
                Dictionary<string, LobbyState> updatedLobbies = CopyLobbies(current);

                updatedLobbies[currentLobby.Code] = updatedLobby;

                if (TryReplaceSnapshot(current, updatedLobbies))
                {
                    lobbyState = updatedLobby;
                    wasUpdated = true;
                    operationCompleted = true;
                }
            }
        }

        return wasUpdated;
    }

    private bool TryReplaceSnapshot(RegistrySnapshot current, Dictionary<string, LobbyState> updatedLobbies)
    {
        RegistrySnapshot updated = new RegistrySnapshot(updatedLobbies);
        RegistrySnapshot previous = Interlocked.CompareExchange(ref _snapshot, updated, current);
        bool wasReplaced = ReferenceEquals(previous, current);
        return wasReplaced;
    }

    private string GenerateUniqueCode(RegistrySnapshot snapshot)
    {
        string code = _codeGenerator.Generate();

        while (snapshot.LobbiesByCode.ContainsKey(code))
        {
            code = _codeGenerator.Generate();
        }

        return code;
    }

    private static bool ContainsPlayer(RegistrySnapshot snapshot, int playerId)
    {
        bool isPresent = TryFindLobby(snapshot, playerId, out _);
        return isPresent;
    }

    private static bool TryFindLobby(RegistrySnapshot snapshot, int playerId, out LobbyState lobby)
    {
        lobby = LobbyState.Empty;
        bool wasFound = false;

        foreach (LobbyState candidate in snapshot.LobbiesByCode.Values)
        {
            if (!wasFound && candidate.Roster.Contains(playerId))
            {
                lobby = candidate;
                wasFound = true;
            }
        }

        return wasFound;
    }

    private static Dictionary<string, LobbyState> CopyLobbies(RegistrySnapshot snapshot)
    {
        Dictionary<string, LobbyState> copiedLobbies =
            new Dictionary<string, LobbyState>(snapshot.LobbiesByCode, StringComparer.Ordinal);
        return copiedLobbies;
    }

    private sealed class RegistrySnapshot
    {
        static RegistrySnapshot()
        {
            Empty = new RegistrySnapshot(new Dictionary<string, LobbyState>(StringComparer.Ordinal));
        }

        public RegistrySnapshot(Dictionary<string, LobbyState> lobbiesByCode)
        {
            LobbiesByCode = lobbiesByCode ?? throw new ArgumentNullException(nameof(lobbiesByCode));
        }

        public static RegistrySnapshot Empty { get; }

        public Dictionary<string, LobbyState> LobbiesByCode { get; }
    }
}
