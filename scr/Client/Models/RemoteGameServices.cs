using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Callbacks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoTrainService;
using DominoTrainGame.Client.Models.Errors;

namespace DominoTrainGame.Client.Models;

public sealed class RemoteGameServices : IGameServices
{
    public async Task<LoginStatus> LogInAsync(string identifier, string password)
    {
        AuthenticationCallback callback = new AuthenticationCallback();
        LoginStatus status = await WithFallback(
            () => InvokeAsync(() => new AuthenticationServiceClient(new InstanceContext(callback)),
                proxy => callback.Login.InvokeAsync(proxy.InnerChannel,
                    () => proxy.LogInAsync(callback.Login.RequestId, identifier, password))),
            LoginStatus.SystemError).ConfigureAwait(false);

        return status;
    }

    public async Task<RegistrationStatus> RegisterUserAsync(User user)
    {
        AuthenticationCallback callback = new AuthenticationCallback();
        RegistrationStatus status = await WithFallback(
            () => InvokeAsync(() => new AuthenticationServiceClient(new InstanceContext(callback)),
                proxy => callback.Registration.InvokeAsync(proxy.InnerChannel,
                    () => proxy.RegisterUserAsync(callback.Registration.RequestId, user))),
            RegistrationStatus.SystemError).ConfigureAwait(false);

        return status;
    }

    public async Task<RegistrationStatus> CheckAvailabilityAsync(User user)
    {
        AuthenticationCallback callback = new AuthenticationCallback();
        RegistrationStatus status = await WithFallback(
            () => InvokeAsync(() => new AuthenticationServiceClient(new InstanceContext(callback)),
                proxy => callback.Availability.InvokeAsync(proxy.InnerChannel,
                    () => proxy.CheckAvailabilityAsync(callback.Availability.RequestId, user))),
            RegistrationStatus.SystemError).ConfigureAwait(false);

        return status;
    }

    public async Task<VerificationSessionDto> BeginVerificationAsync(BeginVerificationRequest request)
    {
        VerificationCallback callback = new VerificationCallback();
        VerificationSessionDto session = await WithFallback(
            () => InvokeAsync(() => new VerificationServiceClient(new InstanceContext(callback)),
                proxy => callback.Started.InvokeAsync(proxy.InnerChannel,
                    () => proxy.BeginVerificationAsync(callback.Started.RequestId, request))),
            new VerificationSessionDto()).ConfigureAwait(false);

        return session;
    }

    public async Task<bool> ResendVerificationAsync(Guid sessionId)
    {
        VerificationCallback callback = new VerificationCallback();
        bool wasSent = await WithFallback(
            () => InvokeAsync(() => new VerificationServiceClient(new InstanceContext(callback)),
                proxy => callback.Resent.InvokeAsync(proxy.InnerChannel,
                    () => proxy.ResendVerificationAsync(callback.Resent.RequestId, sessionId))),
            false).ConfigureAwait(false);

        return wasSent;
    }

    public async Task<VerificationStatus> VerifyCodeAsync(Guid sessionId, string code)
    {
        VerificationCallback callback = new VerificationCallback();
        VerificationStatus status = await WithFallback(
            () => InvokeAsync(() => new VerificationServiceClient(new InstanceContext(callback)),
                proxy => callback.Verified.InvokeAsync(proxy.InnerChannel,
                    () => proxy.VerifyCodeAsync(callback.Verified.RequestId, sessionId, code))),
            VerificationStatus.SystemError).ConfigureAwait(false);

        return status;
    }

    public async Task<List<LeaderboardEntry>> GetEntriesAsync(CancellationToken cancellationToken)
    {
        LeaderboardCallback callback = new LeaderboardCallback();
        LeaderboardResponse result = await InvokeAsync(() => new LeaderboardServiceClient(new InstanceContext(callback)),
            proxy => callback.Entries.InvokeAsync(proxy.InnerChannel,
                () => proxy.GetLeaderboardAsync(callback.Entries.RequestId), cancellationToken),
            cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            throw new GameServiceException("The leaderboard could not be loaded.");
        }
        List<LeaderboardEntry> entries = result.Entries is null
            ? new List<LeaderboardEntry>() : result.Entries.ToList();

        return entries;
    }

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
        catch (CommunicationException exception)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new GameServiceException("The Domino Train server could not complete the request.", exception);
        }
        catch (TimeoutException exception)
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
        T result;

        try
        {
            result = await request().ConfigureAwait(false);
        }
        catch (GameServiceException exception)
        {
            Trace.TraceError(exception.ToString());
            result = fallback;
        }

        return result;
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
