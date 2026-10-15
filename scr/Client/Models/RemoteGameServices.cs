using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoTrainService;
using DominoTrainGame.Client.Models.Errors;

namespace DominoTrainGame.Client.Models;

public sealed class RemoteGameServices : IGameServices
{
    public async Task<LoginValidationStatus> LogInAsync(string identifier, string password) =>
        (await WithFallback(() => InvokeAsync(() => new AuthenticationServiceClient(),
            proxy => proxy.LogInAsync(new LoginRequest { Identifier = identifier, Password = password })),
            new LoginResponse { Status = LoginValidationStatus.DatabaseError }).ConfigureAwait(false)).Status;

    public async Task<SignUpValidationStatus> CheckAvailabilityAsync(string username, string email, string password) =>
        (await WithFallback(() => InvokeAsync(() => new AuthenticationServiceClient(),
            proxy => proxy.CheckAvailabilityAsync(CreateRegistration(username, email, password, Guid.Empty))),
            new SignUpResponse { Status = SignUpValidationStatus.DatabaseError }).ConfigureAwait(false)).Status;

    public async Task<SignUpValidationStatus> RegisterUserAsync(string username, string email, string password, Guid verificationId) =>
        (await WithFallback(() => InvokeAsync(() => new AuthenticationServiceClient(),
            proxy => proxy.RegisterUserAsync(CreateRegistration(username, email, password, verificationId))),
            new SignUpResponse { Status = SignUpValidationStatus.DatabaseError }).ConfigureAwait(false)).Status;

    public Task<VerificationSessionDto> BeginVerificationAsync(string email) =>
        WithFallback(() => InvokeAsync(() => new VerificationServiceClient(),
            proxy => proxy.BeginVerificationAsync(new BeginVerificationRequest
            {
                Email = email, Language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
            })), new VerificationSessionDto());

    public async Task<bool> ResendVerificationAsync(Guid verificationId) =>
        (await WithFallback(() => InvokeAsync(() => new VerificationServiceClient(),
            proxy => proxy.ResendVerificationAsync(new ResendVerificationRequest { SessionId = verificationId })),
            new VerificationSessionDto()).ConfigureAwait(false)).WasSent;

    public async Task<VerificationCodeStatus> VerifyCodeAsync(Guid verificationId, string enteredCode) =>
        (await WithFallback(() => InvokeAsync(() => new VerificationServiceClient(),
            proxy => proxy.VerifyCodeAsync(new VerifyCodeRequest { SessionId = verificationId, Code = enteredCode })),
            new VerificationCodeResponse { Status = VerificationCodeStatus.ServiceUnavailable }).ConfigureAwait(false)).Status;

    public async Task<List<LeaderboardEntry>> GetEntriesAsync(CancellationToken cancellationToken)
    {
        LeaderboardResponse result = await InvokeAsync(() => new LeaderboardServiceClient(),
            proxy => proxy.GetLeaderboardAsync(), cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            throw new GameServiceException("The leaderboard could not be loaded.");
        }
        return result.Entries ?? new List<LeaderboardEntry>();
    }

    private static RegistrationRequest CreateRegistration(string username, string email, string password, Guid verificationId) =>
        new RegistrationRequest
        {
            Username = username, Email = email, Password = password, VerificationId = verificationId,
            Language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
        };

    private static Task<TResult> InvokeAsync<TProxy, TResult>(Func<TProxy> create, Func<TProxy, Task<TResult>> request,
        CancellationToken cancellationToken = default) where TProxy : class, ICommunicationObject => Task.Run(async () =>
    {
        cancellationToken.ThrowIfCancellationRequested();
        TProxy proxy = null;
        try
        {
            proxy = create();
            using (cancellationToken.Register(() => proxy.Abort()))
            {
                TResult result = await request(proxy).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                Close(proxy);
                return result;
            }
        }
        catch (Exception exception) when (exception is CommunicationException || exception is TimeoutException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new GameServiceException("The Domino Train server could not complete the request.", exception);
        }
        finally
        {
            if (proxy != null && proxy.State != CommunicationState.Closed) { proxy.Abort(); }
        }
    }, cancellationToken);

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
