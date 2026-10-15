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
    private readonly object _sessionSync = new object();
    private readonly IEmailSender _email;
    private readonly Func<DateTime> _utcNow;

    public VerificationUseCases(IEmailSender email) : this(email, GetUtcNow) { }

    public VerificationUseCases(IEmailSender email, Func<DateTime> utcNow)
    {
        _email = email ?? throw new ArgumentNullException(nameof(email));
        _utcNow = utcNow ?? throw new ArgumentNullException(nameof(utcNow));
    }

    public async Task<Guid> BeginAsync(string email, string language)
    {
        Guid sessionId = Guid.Empty;
        string address = email?.Trim();

        if (IsValidEmail(address))
        {
            Guid candidate = CreateSession(address, language);
            if (_sessions.TryGetValue(candidate, out VerificationState state)
                && await SendAsync(state).ConfigureAwait(false))
            {
                sessionId = candidate;
            }
        }

        return sessionId;
    }

    public async Task<bool> ResendAsync(Guid sessionId)
    {
        bool wasSent = false;
        if (_sessions.TryGetValue(sessionId, out VerificationState state))
        {
            wasSent = await SendAsync(state).ConfigureAwait(false);
        }

        return wasSent;
    }

    public VerificationCodeStatus Verify(Guid sessionId, string code)
    {
        VerificationCodeStatus status = VerificationCodeStatus.Expired;
        if (_sessions.TryGetValue(sessionId, out VerificationState state))
        {
            lock (state)
            {
                if (!state.IsConsumed)
                {
                    status = state.IsSending || !state.WasSent
                        ? VerificationCodeStatus.Invalid : state.Code.Verify(code);
                    state.IsVerified = status == VerificationCodeStatus.Valid;
                }
            }
        }

        return status;
    }

    public SignUpValidationStatus ExecuteVerified(Guid sessionId, string email, Func<SignUpValidationStatus> register)
    {
        SignUpValidationStatus status = SignUpValidationStatus.VerificationRequired;
        if (_sessions.TryGetValue(sessionId, out VerificationState state))
        {
            lock (state)
            {
                if (!state.IsConsumed && state.IsVerified && !state.IsSending
                    && _utcNow() < state.ExpiresAtUtc
                    && string.Equals(email?.Trim(), state.Email, StringComparison.OrdinalIgnoreCase))
                {
                    status = register();
                    if (status == SignUpValidationStatus.Success)
                    {
                        state.IsVerified = false;
                        state.WasSent = false;
                        state.IsConsumed = true;
                        _sessions.TryRemove(sessionId, out _);
                    }
                }
            }
        }

        return status;
    }

    private Guid CreateSession(string email, string language)
    {
        Guid sessionId = Guid.Empty;
        lock (_sessionSync)
        {
            RemoveExpiredSessions();
            bool canStart = true;
            foreach (var entry in _sessions)
            {
                if (string.Equals(entry.Value.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    lock (entry.Value)
                    {
                        canStart = !entry.Value.IsSending
                            && entry.Value.LastSentAtUtc != DateTime.MinValue
                            && (_utcNow() - entry.Value.LastSentAtUtc).TotalSeconds >= ResendCooldownSeconds;
                        if (canStart)
                        {
                            entry.Value.IsConsumed = true;
                            _sessions.TryRemove(entry.Key, out _);
                        }
                    }
                    break;
                }
            }

            if (canStart && _sessions.Count < MaximumActiveSessions)
            {
                sessionId = Guid.NewGuid();
                _sessions[sessionId] = new VerificationState(email, language, _utcNow);
            }
        }

        return sessionId;
    }

    private async Task<bool> SendAsync(VerificationState state)
    {
        bool wasSent = false;
        string code = null;
        lock (state)
        {
            DateTime now = _utcNow();
            if (!state.IsConsumed && !state.IsSending && now < state.ExpiresAtUtc
                && (now - state.LastSentAtUtc).TotalSeconds >= ResendCooldownSeconds)
            {
                code = state.Code.GenerateCode();
                state.IsSending = true;
                state.IsVerified = false;
                state.WasSent = false;
                state.LastSentAtUtc = now;
                state.ExpiresAtUtc = now.AddMinutes(VerificationCodeService.ValidityMinutes);
            }
        }

        if (code != null)
        {
            try
            {
                wasSent = await _email.SendVerificationCodeAsync(state.Email, code, state.Language).ConfigureAwait(false);
                lock (state) { state.WasSent = wasSent; }
            }
            finally
            {
                lock (state) { state.IsSending = false; }
            }
        }

        return wasSent;
    }

    private void RemoveExpiredSessions()
    {
        foreach (var entry in _sessions)
        {
            lock (entry.Value)
            {
                if (!entry.Value.IsSending && _utcNow() >= entry.Value.ExpiresAtUtc)
                {
                    _sessions.TryRemove(entry.Key, out _);
                }
            }
        }
    }

    private static bool IsValidEmail(string email)
    {
        bool isValid = false;
        if (!string.IsNullOrWhiteSpace(email))
        {
            try { isValid = new MailAddress(email).Address == email; }
            catch (FormatException) { isValid = false; }
        }

        return isValid;
    }

    private static DateTime GetUtcNow()
    {
        return DateTime.UtcNow;
    }

    private sealed class VerificationState
    {
        public VerificationState(string email, string language, Func<DateTime> utcNow)
        {
            Email = email;
            Language = language;
            Code = new VerificationCodeService(utcNow);
            ExpiresAtUtc = utcNow().AddMinutes(VerificationCodeService.ValidityMinutes);
        }

        public string Email { get; }
        public string Language { get; }
        public VerificationCodeService Code { get; }
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime LastSentAtUtc { get; set; } = DateTime.MinValue;
        public bool WasSent { get; set; }
        public bool IsSending { get; set; }
        public bool IsVerified { get; set; }
        public bool IsConsumed { get; set; }
    }
}
