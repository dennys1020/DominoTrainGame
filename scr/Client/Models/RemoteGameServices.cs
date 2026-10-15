using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Callbacks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoTrainService;
using DominoTrainGame.Client.Models.Errors;
using DominoTrainGame.Client.Models.Notifications;

namespace DominoTrainGame.Client.Models;

public sealed class RemoteGameServices : IGameServices
{
    public event EventHandler<ServiceOperationEventArgs> OperationChanged;

    public async Task<LoginValidationStatus> LogInAsync(string identifier, string password) =>
        (await WithFallback(() => InvokeAsync(CreateAuthentication, proxy => proxy.LogInAsync(new LoginRequest
        {
            Identifier = identifier, Password = password
        })), new LoginResponse { Status = LoginValidationStatus.DatabaseError }).ConfigureAwait(false)).Status;

    public async Task<SignUpValidationStatus> CheckAvailabilityAsync(string username, string email, string password) =>
        (await WithFallback(() => InvokeAsync(CreateAuthentication, proxy => proxy.CheckAvailabilityAsync(
            CreateRegistration(username, email, password, Guid.Empty))),
            new SignUpResponse { Status = SignUpValidationStatus.DatabaseError }).ConfigureAwait(false)).Status;

    public async Task<SignUpValidationStatus> RegisterUserAsync(string username, string email, string password, Guid verificationId) =>
        (await WithFallback(() => InvokeAsync(CreateAuthentication, proxy => proxy.RegisterUserAsync(
            CreateRegistration(username, email, password, verificationId))),
            new SignUpResponse { Status = SignUpValidationStatus.DatabaseError }).ConfigureAwait(false)).Status;

    public Task<RecoveryAccountDto> FindAccountAsync(string identifier) =>
        WithFallback(() => InvokeAsync(CreateAuthentication, proxy => proxy.FindAccountAsync(new FindAccountRequest
        {
            Identifier = identifier
        })), new RecoveryAccountDto { Status = RecoverPasswordValidationStatus.DatabaseError });

    public async Task<RecoverPasswordValidationStatus> UpdatePasswordAsync(int playerId, string password, string confirmation, Guid verificationId) =>
        (await WithFallback(() => InvokeAsync(CreateAuthentication, proxy => proxy.UpdatePasswordAsync(new PasswordResetRequest
        {
            PlayerId = playerId, Password = password, Confirmation = confirmation, VerificationId = verificationId
        })), new PasswordResetResponse { Status = RecoverPasswordValidationStatus.DatabaseError }).ConfigureAwait(false)).Status;

    public Task<VerificationSessionDto> BeginVerificationAsync(string email)
    {
        Guid requestId = Guid.NewGuid();
        VerificationCallback callback = new VerificationCallback(requestId, Report);
        return WithFallback(() => InvokeAsync(() => CreateVerification(callback), proxy => AwaitNotificationAsync(
            proxy, callback, () => proxy.BeginVerificationAsync(new BeginVerificationRequest
            {
                RequestId = requestId, Email = email, Language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
            }), CancellationToken.None)), new VerificationSessionDto());
    }

    public async Task<bool> ResendVerificationAsync(Guid verificationId)
    {
        Guid requestId = Guid.NewGuid();
        VerificationCallback callback = new VerificationCallback(requestId, Report);
        VerificationSessionDto result = await WithFallback(() => InvokeAsync(() => CreateVerification(callback),
            proxy => AwaitNotificationAsync(proxy, callback, () => proxy.ResendVerificationAsync(new ResendVerificationRequest
            {
                RequestId = requestId, SessionId = verificationId
            }), CancellationToken.None)), new VerificationSessionDto()).ConfigureAwait(false);
        return result.WasSent;
    }

    public async Task<VerificationCodeStatus> VerifyCodeAsync(Guid verificationId, string enteredCode)
    {
        VerificationCallback callback = new VerificationCallback(Guid.Empty, Report);
        VerificationCodeResponse result = await WithFallback(() => InvokeAsync(() => CreateVerification(callback),
            proxy => proxy.VerifyCodeAsync(new VerifyCodeRequest { SessionId = verificationId, Code = enteredCode })),
            new VerificationCodeResponse { Status = VerificationCodeStatus.ServiceUnavailable }).ConfigureAwait(false);
        return result.Status;
    }

    public async Task<List<LeaderboardEntry>> GetEntriesAsync(CancellationToken cancellationToken)
    {
        Guid requestId = Guid.NewGuid();
        LeaderboardCallback callback = new LeaderboardCallback(requestId, Report);
        LeaderboardResponse result = await InvokeAsync(() => CreateLeaderboard(callback),
            proxy => AwaitNotificationAsync(proxy, callback,
                () => proxy.GetLeaderboardAsync(new LeaderboardRequest { RequestId = requestId }), cancellationToken),
            cancellationToken).ConfigureAwait(false);
        return result.Entries?.ToList() ?? new List<LeaderboardEntry>();
    }

    private static AuthenticationServiceClient CreateAuthentication() => new AuthenticationServiceClient();

    private static VerificationServiceClient CreateVerification(VerificationCallback callback) =>
        new VerificationServiceClient(new InstanceContext(callback));

    private static LeaderboardServiceClient CreateLeaderboard(LeaderboardCallback callback) =>
        new LeaderboardServiceClient(new InstanceContext(callback));

    private static RegistrationRequest CreateRegistration(string username, string email, string password, Guid verificationId) =>
        new RegistrationRequest
        {
            Username = username, Email = email, Password = password, VerificationId = verificationId,
            Language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
        };

    private static Task<TResult> InvokeAsync<TProxy, TResult>(Func<TProxy> create, Func<TProxy, Task<TResult>> request,
        CancellationToken cancellationToken = default) where TProxy : class, ICommunicationObject => Task.Run(async () =>
        {
            TProxy proxy = null;
            bool completed = false;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                proxy = create();
                TResult result = await request(proxy).ConfigureAwait(false);
                completed = true;
                return result;
            }
            catch (Exception exception) when (exception is CommunicationException || exception is TimeoutException)
            {
                throw new GameServiceException("The Domino Train server could not complete the request.", exception);
            }
            finally
            {
                if (proxy != null)
                {
                    if (!completed || cancellationToken.IsCancellationRequested) { proxy.Abort(); }
                    else { Close(proxy); }
                }
            }
        }, cancellationToken);

    private static async Task<TResult> AwaitNotificationAsync<TResult>(ICommunicationObject proxy,
        OperationCallback<TResult> callback, Func<Task> send, CancellationToken cancellationToken)
    {
        EventHandler disconnected = (_, __) => callback.Fail(new GameServiceException(
            "The connection closed before the server completed the operation."));
        proxy.Faulted += disconnected;
        proxy.Closed += disconnected;
        using (CancellationTokenSource timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
        using (timeout.Token.Register(() => callback.Fail(new GameServiceException(
            "The server did not notify the result before the operation timed out."))))
        using (cancellationToken.Register(() => callback.Cancel()))
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await send().ConfigureAwait(false);
                return await callback.Completion.ConfigureAwait(false);
            }
            finally
            {
                proxy.Faulted -= disconnected;
                proxy.Closed -= disconnected;
            }
        }
    }

    private void Report(Guid requestId, OperationState state, string message)
    {
        Trace.TraceInformation("{0}: {1} - {2}", requestId, state, message);
        OperationChanged?.Invoke(this, new ServiceOperationEventArgs(requestId, state, message));
    }

    private static async Task<T> WithFallback<T>(Func<Task<T>> request, T fallback)
    {
        try { return await request().ConfigureAwait(false); }
        catch (GameServiceException exception) { Trace.TraceError(exception.ToString()); return fallback; }
    }

    private static void Close(ICommunicationObject connection)
    {
        try
        {
            if (connection.State == CommunicationState.Faulted) { connection.Abort(); }
            else { connection.Close(); }
        }
        catch (CommunicationException) { connection.Abort(); }
        catch (TimeoutException) { connection.Abort(); }
    }
}
