using System;
using System.ServiceModel;
using System.Threading.Tasks;
using GeneratedAuthenticationCallback = Models.DominoService.IAuthenticationServiceCallback;
using GeneratedLoginResponse = Models.DominoService.LoginResponse;
using GeneratedLoginStatus = Models.DominoService.LoginStatus;

namespace DominoTrainGame.Client.Models.Callbacks;

[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
internal sealed class AuthenticationCallback : GeneratedAuthenticationCallback
{
    private readonly TaskCompletionSource<GeneratedLoginStatus> _loginCompletion;

    public AuthenticationCallback()
    {
        RequestId = Guid.NewGuid();
        _loginCompletion = new TaskCompletionSource<GeneratedLoginStatus>(
            TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public Guid RequestId { get; }

    public void OnLogInCompleted(GeneratedLoginResponse response)
    {
        bool isExpectedResponse = response != null && response.RequestId == RequestId;

        if (isExpectedResponse)
        {
            _loginCompletion.TrySetResult(response.Status);
        }
    }

    public async Task<GeneratedLoginStatus> WaitForLogInAsync(TimeSpan timeout)
    {
        Task completedTask = await Task.WhenAny(_loginCompletion.Task, Task.Delay(timeout)).ConfigureAwait(false);

        if (completedTask != _loginCompletion.Task)
        {
            throw new TimeoutException("The server did not send the login result before the configured timeout.");
        }

        GeneratedLoginStatus loginStatus = await _loginCompletion.Task.ConfigureAwait(false);
        return loginStatus;
    }
}
