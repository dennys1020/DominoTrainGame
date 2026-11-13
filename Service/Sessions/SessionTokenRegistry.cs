using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Service.Sessions;

public sealed class SessionTokenRegistry
{
    private const int TokenByteLength = 32;
    private const int TokenLifetimeMinutes = 30;

    private readonly Dictionary<string, IssuedSession> _sessionsByToken;
    private readonly object _sessionsLock;

    public SessionTokenRegistry()
    {
        _sessionsByToken = new Dictionary<string, IssuedSession>(StringComparer.Ordinal);
        _sessionsLock = new object();
    }

    public string Issue(PlayerIdentity player)
    {
        if (player == null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        string token = GenerateToken();
        Store(token, player);
        return token;
    }

    public PlayerIdentity Resolve(string token)
    {
        string normalizedToken = token ?? string.Empty;
        PlayerIdentity player = PlayerIdentity.Empty;

        lock (_sessionsLock)
        {
            if (_sessionsByToken.ContainsKey(normalizedToken))
            {
                player = ResolveStoredSession(normalizedToken);
            }
        }

        return player;
    }

    public void Renew(string token, PlayerIdentity player)
    {
        if (player == null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        if (!string.IsNullOrEmpty(token))
        {
            Store(token, player);
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

    private PlayerIdentity ResolveStoredSession(string token)
    {
        IssuedSession session = _sessionsByToken[token];
        bool isExpired = session.ExpiresAtUtc <= DateTime.UtcNow;
        PlayerIdentity player = isExpired ? PlayerIdentity.Empty : session.Player;

        if (isExpired)
        {
            _sessionsByToken.Remove(token);
        }

        return player;
    }

    private void Store(string token, PlayerIdentity player)
    {
        DateTime expiresAtUtc = DateTime.UtcNow.AddMinutes(TokenLifetimeMinutes);

        lock (_sessionsLock)
        {
            _sessionsByToken[token] = new IssuedSession(player, expiresAtUtc);
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
