using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Service.Sessions;

public sealed class SessionTokenRegistry
{
    private const int TokenByteLength = 32;
    private const int TokenLifetimeMinutes = 30;

    private readonly ConcurrentDictionary<string, IssuedSession> _sessionsByToken;

    public SessionTokenRegistry()
    {
        _sessionsByToken = new ConcurrentDictionary<string, IssuedSession>(StringComparer.Ordinal);
    }

    public string Issue(PlayerIdentity player)
    {
        if (player == null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        string token = GenerateToken();
        DateTime expiresAtUtc = DateTime.UtcNow.AddMinutes(TokenLifetimeMinutes);

        _sessionsByToken[token] = new IssuedSession(player, expiresAtUtc);

        return token;
    }

    public bool TryResolve(string token, out PlayerIdentity player)
    {
        bool isValid = false;

        player = PlayerIdentity.Empty;

        if (!string.IsNullOrEmpty(token) && _sessionsByToken.TryGetValue(token, out IssuedSession session))
        {
            if (session.ExpiresAtUtc > DateTime.UtcNow)
            {
                player = session.Player;
                isValid = true;
            }
            else
            {
                _sessionsByToken.TryRemove(token, out IssuedSession expiredSession);
            }
        }

        return isValid;
    }

    public void Renew(string token, PlayerIdentity player)
    {
        if (player == null)
        {
            throw new ArgumentNullException(nameof(player));
        }

        if (!string.IsNullOrEmpty(token))
        {
            DateTime expiresAtUtc = DateTime.UtcNow.AddMinutes(TokenLifetimeMinutes);
            _sessionsByToken[token] = new IssuedSession(player, expiresAtUtc);
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
