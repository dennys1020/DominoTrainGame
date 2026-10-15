using System;
using System.Collections.Concurrent;
using System.Net.Mail;
using System.Threading.Tasks;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.Verification;

public sealed class VerificationUseCases
{
    private const int ResendCooldownSeconds = 30;
    private const int MaximumActiveSessions = 10000;
    private readonly ConcurrentDictionary<Guid, VerificationState> _sessions = new ConcurrentDictionary<Guid, VerificationState>();
    private readonly IEmailSender _email;

    public VerificationUseCases(IEmailSender email) => _email = email;

    public async Task<Guid> BeginAsync(string email, string language)
    {
        if (!IsValidEmail(email))
        {
            return Guid.Empty;
        }
        RemoveExpiredSessions();
        if (_sessions.Count >= MaximumActiveSessions)
        {
            return Guid.Empty;
        }

        Guid sessionId = Guid.NewGuid();
        VerificationState state = new VerificationState(email, language);
        _sessions[sessionId] = state;
        if (await SendAsync(state).ConfigureAwait(false))
        {
            return sessionId;
        }
        _sessions.TryRemove(sessionId, out _);
        return Guid.Empty;
    }

    public Task<bool> ResendAsync(Guid sessionId)
    {
        return _sessions.TryGetValue(sessionId, out VerificationState state)
            ? SendAsync(state) : Task.FromResult(false);
    }

    public VerificationCodeStatus Verify(Guid sessionId, string code)
    {
        if (!_sessions.TryGetValue(sessionId, out VerificationState state))
        {
            return VerificationCodeStatus.Expired;
        }
        lock (state)
        {
            if (state.IsConsumed)
            {
                return VerificationCodeStatus.Expired;
            }
            if (state.IsSending || !state.WasSent)
            {
                return VerificationCodeStatus.Invalid;
            }
            VerificationCodeStatus status = state.Code.Verify(code);
            state.IsVerified = status == VerificationCodeStatus.Valid;
            return status;
        }
    }

    // A verified session is bound to its email and can authorize one successful write.
    public T ExecuteVerified<T>(Guid sessionId, string email, Func<T> execute, Predicate<T> succeeded, T rejected)
    {
        if (!_sessions.TryGetValue(sessionId, out VerificationState state))
        {
            return rejected;
        }
        lock (state)
        {
            if (state.IsConsumed || !state.IsVerified || state.IsSending || DateTime.UtcNow >= state.ExpiresAtUtc
                || !string.Equals(email, state.Email, StringComparison.OrdinalIgnoreCase))
            {
                return rejected;
            }
            T result = execute();
            if (succeeded(result))
            {
                state.IsVerified = false;
                state.WasSent = false;
                state.IsConsumed = true;
                _sessions.TryRemove(sessionId, out _);
            }
            return result;
        }
    }

    private async Task<bool> SendAsync(VerificationState state)
    {
        string code;
        lock (state)
        {
            if (state.IsConsumed || state.IsSending
                || (DateTime.UtcNow - state.LastSentAtUtc).TotalSeconds < ResendCooldownSeconds)
            {
                return false;
            }
            state.IsSending = true;
            state.IsVerified = false;
            state.WasSent = false;
            code = state.Code.GenerateCode();
            state.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(VerificationCodeService.ValidityMinutes);
        }
        try
        {
            bool wasSent = await _email.SendVerificationCodeAsync(state.Email, code, state.Language).ConfigureAwait(false);
            lock (state)
            {
                state.WasSent = wasSent;
                if (wasSent)
                {
                    state.LastSentAtUtc = DateTime.UtcNow;
                }
            }
            return wasSent;
        }
        finally
        {
            lock (state)
            {
                state.IsSending = false;
            }
        }
    }

    private void RemoveExpiredSessions()
    {
        foreach (var entry in _sessions)
        {
            lock (entry.Value)
            {
                if (!entry.Value.IsSending && DateTime.UtcNow >= entry.Value.ExpiresAtUtc)
                {
                    _sessions.TryRemove(entry.Key, out _);
                }
            }
        }
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }
        try
        {
            return new MailAddress(email).Address == email;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private sealed class VerificationState
    {
        public VerificationState(string email, string language) { Email = email; Language = language; }
        public string Email { get; }
        public string Language { get; }
        public VerificationCodeService Code { get; } = new VerificationCodeService();
        public DateTime ExpiresAtUtc { get; set; } = DateTime.UtcNow.AddMinutes(VerificationCodeService.ValidityMinutes);
        public DateTime LastSentAtUtc { get; set; } = DateTime.MinValue;
        public bool WasSent { get; set; }
        public bool IsSending { get; set; }
        public bool IsVerified { get; set; }
        public bool IsConsumed { get; set; }
    }
}
