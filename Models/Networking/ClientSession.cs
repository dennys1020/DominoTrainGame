using System;
using System.Collections.Generic;
using System.Threading;
using DominoTrainGame.Client.Models.Contracts;

namespace DominoTrainGame.Client.Models.Networking;

public sealed class ClientSession
{
    private const int NoPlayerId = 0;

    private readonly object _sessionSyncRoot = new object();
    private readonly List<Func<LobbyResult, LobbyResult>> _pendingLobbyUpdates =
        new List<Func<LobbyResult, LobbyResult>>();
    private LobbyResult _currentLobby = LobbyResult.Empty;
    private int _currentPlayerId;
    private string _sessionToken = string.Empty;
    private SynchronizationContext _callerContext = new SynchronizationContext();
    private MatchSnapshot _matchSnapshot = MatchSnapshot.Empty;
    private PlayerHand _playerHand = PlayerHand.Empty;

    public LobbyResult CurrentLobby
    {
        get
        {
            LobbyResult currentLobby;

            lock (_sessionSyncRoot)
            {
                currentLobby = _currentLobby;
            }

            return currentLobby;
        }
    }

    public MatchSnapshot CurrentMatch
    {
        get
        {
            MatchSnapshot match;

            lock (_sessionSyncRoot)
            {
                match = _matchSnapshot;
            }

            return match;
        }
    }

    public PlayerHand CurrentHand
    {
        get
        {
            PlayerHand playerHand;

            lock (_sessionSyncRoot)
            {
                playerHand = _playerHand;
            }

            return playerHand;
        }
    }

    public int CurrentPlayerId
    {
        get
        {
            int currentPlayerId;

            lock (_sessionSyncRoot)
            {
                currentPlayerId = _currentPlayerId;
            }

            return currentPlayerId;
        }
    }

    public string SessionToken
    {
        get
        {
            string sessionToken;

            lock (_sessionSyncRoot)
            {
                sessionToken = _sessionToken;
            }

            return sessionToken;
        }
    }

    public SynchronizationContext CallerContext
    {
        get
        {
            SynchronizationContext callerContext;

            lock (_sessionSyncRoot)
            {
                callerContext = _callerContext;
            }

            return callerContext;
        }
    }

    public void Store(int playerId, string sessionToken, SynchronizationContext callerContext)
    {
        lock (_sessionSyncRoot)
        {
            _callerContext = callerContext ?? _callerContext;
            _currentPlayerId = playerId;
            _sessionToken = sessionToken ?? string.Empty;
        }
    }

    public void Forget()
    {
        lock (_sessionSyncRoot)
        {
            _sessionToken = string.Empty;
            _currentPlayerId = NoPlayerId;
        }
    }

    public LobbyResult StoreLobby(LobbyResult lobby)
    {
        if (lobby == null)
        {
            throw new ArgumentNullException(nameof(lobby));
        }

        LobbyResult currentLobby;

        lock (_sessionSyncRoot)
        {
            _currentLobby = lobby;

            foreach (Func<LobbyResult, LobbyResult> pendingUpdate in _pendingLobbyUpdates)
            {
                _currentLobby = pendingUpdate(_currentLobby);
            }

            _pendingLobbyUpdates.Clear();
            currentLobby = _currentLobby;
        }

        return currentLobby;
    }

    public void StoreMatch(MatchSnapshot matchSnapshot)
    {
        if (matchSnapshot == null)
        {
            throw new ArgumentNullException(nameof(matchSnapshot));
        }

        lock (_sessionSyncRoot)
        {
            _matchSnapshot = matchSnapshot;
        }
    }

    public void StoreHand(PlayerHand hand)
    {
        if (hand == null)
        {
            throw new ArgumentNullException(nameof(hand));
        }

        lock (_sessionSyncRoot)
        {
            _playerHand = hand;
        }
    }

    public void ClearLobby()
    {
        lock (_sessionSyncRoot)
        {
            _currentLobby = LobbyResult.Empty;
            _matchSnapshot = MatchSnapshot.Empty;
            _playerHand = PlayerHand.Empty;
            _pendingLobbyUpdates.Clear();
        }
    }

    public bool TryApplyLobbyUpdate(long version, Func<LobbyResult, LobbyResult> update)
    {
        if (update == null)
        {
            throw new ArgumentNullException(nameof(update));
        }

        bool wasApplied;

        lock (_sessionSyncRoot)
        {
            bool hasLobby = _currentLobby.Status == LobbyResultStatus.Success;
            wasApplied = hasLobby && version > _currentLobby.Version;

            if (!hasLobby)
            {
                _pendingLobbyUpdates.Add(lobby => LobbyRosterUpdates.ApplyIfNewer(lobby, version, update));
            }
            else if (wasApplied)
            {
                _currentLobby = update(_currentLobby);
            }
        }

        return wasApplied;
    }

    public ReconnectionPlan PrepareReconnection()
    {
        ReconnectionPlan reconnectionPlan;

        lock (_sessionSyncRoot)
        {
            bool canReconnect = !string.IsNullOrEmpty(_sessionToken);
            string lobbyCode = canReconnect ? _currentLobby.Code : string.Empty;
            reconnectionPlan = new ReconnectionPlan(canReconnect, lobbyCode);
            _currentLobby = LobbyResult.Empty;
            _pendingLobbyUpdates.Clear();

            if (!canReconnect)
            {
                _currentPlayerId = NoPlayerId;
            }
        }

        return reconnectionPlan;
    }
}
