using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Service.Sessions;

public sealed class SessionTokenRegistry
{
    private const int TokenByteLength = 32;
    private const int TokenLifetimeMinutes = 30;

    private readonly IClock _clock;
    private readonly Dictionary<string, IssuedSession> _sessionsByToken;
    private readonly Dictionary<int, string> _tokensByPlayerId;
    private readonly object _sessionsLock;

    public SessionTokenRegistry(IClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _sessionsByToken = new Dictionary<string, IssuedSession>(StringComparer.Ordinal);
        _tokensByPlayerId = new Dictionary<int, string>();
        _sessionsLock = new object();
    }

    public string Issue(PlayerIdentity player)
    {
        if (player == null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        string token = GenerateToken();

        lock (_sessionsLock)
        {
            ForgetPlayerToken(player.PlayerId);
            StoreRenewedSession(token, player);
            _tokensByPlayerId[player.PlayerId] = token;
        }

        return token;
    }

    public PlayerIdentity Resolve(string token)
    {
        string normalizedToken = token ?? string.Empty;
        PlayerIdentity player = PlayerIdentity.Empty;

        lock (_sessionsLock)
        {
            if (IsActive(normalizedToken))
            {
                player = _sessionsByToken[normalizedToken].Player;
            }
        }

        return player;
    }

    public void Renew(string token)
    {
        string normalizedToken = token ?? string.Empty;

        lock (_sessionsLock)
        {
            if (_sessionsByToken.ContainsKey(normalizedToken))
            {
                StoreRenewedSession(normalizedToken, _sessionsByToken[normalizedToken].Player);
            }
        }
    }

    public void UpdateUsername(int playerId, string newUsername)
    {
        lock (_sessionsLock)
        {
            if (_tokensByPlayerId.ContainsKey(playerId))
            {
                PlayerIdentity renamedPlayer = new PlayerIdentity(playerId, newUsername);
                StoreRenewedSession(_tokensByPlayerId[playerId], renamedPlayer);
            }
        }
    }

    public void RevokePlayer(int playerId)
    {
        lock (_sessionsLock)
        {
            ForgetPlayerToken(playerId);
        }
    }

    private static string GenerateToken()
    {
        byte[] bytes = new byte[TokenByteLength];

        using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
        {
            generator.GetBytes(bytes);
        }

        string token = Convert.ToBase64String(bytes);

        return token;
    }

    private DateTime CalculateExpiration()
    {
        DateTime expiresAtUtc = _clock.UtcNow.AddMinutes(TokenLifetimeMinutes);
        return expiresAtUtc;
    }

    private void StoreRenewedSession(string token, PlayerIdentity player)
    {
        _sessionsByToken[token] = new IssuedSession(player, CalculateExpiration());
    }

    private bool IsActive(string token)
    {
        bool isActive = _sessionsByToken.ContainsKey(token)
            && _sessionsByToken[token].ExpiresAtUtc > _clock.UtcNow;
        return isActive;
    }

    private void ForgetPlayerToken(int playerId)
    {
        if (_tokensByPlayerId.ContainsKey(playerId))
        {
            _sessionsByToken.Remove(_tokensByPlayerId[playerId]);
            _tokensByPlayerId.Remove(playerId);
        }
    }

    private sealed class IssuedSession
    {
        public IssuedSession(PlayerIdentity player, DateTime expiresAtUtc)
        {
            Player = player;
            ExpiresAtUtc = expiresAtUtc;
        }

        public PlayerIdentity Player { get; }

        public DateTime ExpiresAtUtc { get; }
    }
}
