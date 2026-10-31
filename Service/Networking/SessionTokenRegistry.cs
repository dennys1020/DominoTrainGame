using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using DominoTrainGame.Server.Application.Authentication;

namespace DominoTrainGame.Server.Service.Networking;

public sealed class SessionTokenRegistry
{
    private const int TokenByteLength = 32;
    private const int TokenLifetimeMinutes = 30;

    private readonly ConcurrentDictionary<string, IssuedSession> _sessionsByToken;

    public SessionTokenRegistry()
    {
        _sessionsByToken = new ConcurrentDictionary<string, IssuedSession>(StringComparer.Ordinal);
    }

    public string Issue(AuthenticatedPlayer player)
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

    public bool TryResolve(string token, out AuthenticatedPlayer player)
    {
        bool isValid = false;

        player = AuthenticatedPlayer.Empty;

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

    public void Renew(string token, AuthenticatedPlayer player)
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

    public void Revoke(string token)
    {
        if (!string.IsNullOrEmpty(token))
        {
            _sessionsByToken.TryRemove(token, out IssuedSession revokedSession);
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
        public IssuedSession(AuthenticatedPlayer player, DateTime expiresAtUtc)
        {
            Player = player;
            ExpiresAtUtc = expiresAtUtc;
        }

        public AuthenticatedPlayer Player { get; }

        public DateTime ExpiresAtUtc { get; }
    }
}
