using System;
using System.Collections.Generic;
using System.Threading;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Shared.Common;

namespace DominoTrainGame.Client.Models.Networking;

public sealed class ClientSession
{

    private readonly object _sessionSyncRoot = new object();
    private readonly List<Func<LobbyResult, LobbyResult>> _pendingLobbyUpdates =
        new List<Func<LobbyResult, LobbyResult>>();
    private LobbyResult _currentLobby = LobbyResult.Empty;
    private int _currentPlayerId;
    private bool _isGuest;
    private string _sessionToken = string.Empty;
    private string _currentUsername = string.Empty;
    private string _pendingVerificationEmail = string.Empty;
    private string _currentMatchCode = string.Empty;
    private SynchronizationContext _callerContext = new SynchronizationContext();
    private MatchSnapshot _matchSnapshot = MatchSnapshot.Empty;
    private PlayerHand _playerHand = PlayerHand.Empty;
    private List<RoundSummary> _roundHistory = new List<RoundSummary>();

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

    public string CurrentMatchCode
    {
        get
        {
            string currentMatchCode;

            lock (_sessionSyncRoot)
            {
                currentMatchCode = _currentMatchCode;
            }

            return currentMatchCode;
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

    public string CurrentUsername
    {
        get
        {
            string currentUsername;

            lock (_sessionSyncRoot)
            {
                currentUsername = _currentUsername;
            }

            return currentUsername;
        }
    }

    public bool IsGuest
    {
        get
        {
            bool isGuest;

            lock (_sessionSyncRoot)
            {
                isGuest = _isGuest;
            }

            return isGuest;
        }
    }

    public string PendingVerificationEmail
    {
        get
        {
            string pendingVerificationEmail;

            lock (_sessionSyncRoot)
            {
                pendingVerificationEmail = _pendingVerificationEmail;
            }

            return pendingVerificationEmail;
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
            _isGuest = false;
        }
    }

    public void StoreUsername(string username)
    {
        lock (_sessionSyncRoot)
        {
            _currentUsername = username ?? string.Empty;
        }
    }

    public void StorePendingVerificationEmail(string email)
    {
        lock (_sessionSyncRoot)
        {
            _pendingVerificationEmail = email ?? string.Empty;
        }
    }

    public void MarkAsGuest()
    {
        lock (_sessionSyncRoot)
        {
            _isGuest = true;
        }
    }

    public void Forget()
    {
        lock (_sessionSyncRoot)
        {
            _sessionToken = string.Empty;
            _currentUsername = string.Empty;
            _currentPlayerId = StateIdentifiers.EmptyPlayerId;
            _isGuest = false;
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

    public void StoreMatchCode(string matchCode)
    {
        lock (_sessionSyncRoot)
        {
            _currentMatchCode = matchCode ?? string.Empty;
        }
    }

    public void ForgetMatch()
    {
        lock (_sessionSyncRoot)
        {
            _currentMatchCode = string.Empty;
        }
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

    public IReadOnlyList<RoundSummary> RoundHistory
    {
        get
        {
            IReadOnlyList<RoundSummary> roundHistory;

            lock (_sessionSyncRoot)
            {
                roundHistory = new List<RoundSummary>(_roundHistory);
            }

            return roundHistory;
        }
    }

    public void StoreRound(RoundSummary roundSummary)
    {
        if (roundSummary == null)
        {
            throw new ArgumentNullException(nameof(roundSummary));
        }

        lock (_sessionSyncRoot)
        {
            _roundHistory.Add(roundSummary);
        }
    }

    public void ClearRoundHistory()
    {
        lock (_sessionSyncRoot)
        {
            _roundHistory = new List<RoundSummary>();
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
            _currentMatchCode = string.Empty;
            _matchSnapshot = MatchSnapshot.Empty;
            _playerHand = PlayerHand.Empty;
            _pendingLobbyUpdates.Clear();
            _roundHistory = new List<RoundSummary>();
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
            bool hasLobby = _currentLobby.Status == LobbyActionResult.Success;
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
                _currentPlayerId = StateIdentifiers.EmptyPlayerId;
            }
        }

        return reconnectionPlan;
    }
}
