using System;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;
using DominoTrainGame.Client.Models.Networking;
using DominoTrainGame.Shared.Common;
using DominoTrainGame.Shared.Networking;

namespace DominoTrainGame.Client.Models;

public sealed class RemoteProfileServices : IRemoteProfileServices
{
    private const string ProfileOperationName = "profile";

    private static readonly ServiceFailureMapping<ProfileActionResult> _profileFailures;

    private readonly IGameConnectionFactory _connectionFactory;
    private readonly GameChannelCloser _channelCloser;
    private readonly ClientSession _session;
    private readonly RemoteCallGuard _callGuard;

    static RemoteProfileServices()
    {
        _profileFailures = new ServiceFailureMapping<ProfileActionResult>(
            ProfileOperationName,
            ProfileResultMapper.MapFault,
            ProfileActionResult.ServerError,
            ProfileActionResult.ServerUnavailable);
    }

    public RemoteProfileServices(
        IGameConnectionFactory connectionFactory,
        GameChannelCloser channelCloser,
        ClientSession session)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _channelCloser = channelCloser ?? throw new ArgumentNullException(nameof(channelCloser));
        _session = session ?? throw new ArgumentNullException(nameof(session));
        _callGuard = new RemoteCallGuard();
    }

    public string CurrentUsername => _session.CurrentUsername;

    public Task<PlayerProfileResult> GetProfileAsync(string username)
    {
        ProfileRequest request = new ProfileRequest
        {
            SessionToken = _session.SessionToken,
            Username = username
        };
        ProfileCall<ProfileRequest, PlayerProfileResult> profileCall =
            new ProfileCall<ProfileRequest, PlayerProfileResult>(this, request, RequestProfileAsync);
        Task<PlayerProfileResult> loading = _callGuard.ExecuteAsync(
            profileCall.SendAsync,
            _profileFailures,
            PlayerProfileResult.CreateFailure);
        return loading;
    }

    public Task<ProfileActionResult> UpdateBiographyAsync(string biography)
    {
        BiographyUpdateRequest request = new BiographyUpdateRequest
        {
            SessionToken = _session.SessionToken,
            Biography = biography
        };
        ProfileCall<BiographyUpdateRequest, ProfileActionResult> biographyCall =
            new ProfileCall<BiographyUpdateRequest, ProfileActionResult>(this, request, SendBiographyAsync);
        Task<ProfileActionResult> updating = _callGuard.ExecuteAsync(biographyCall.SendAsync, _profileFailures);
        return updating;
    }

    public Task<ProfileActionResult> ChangeUsernameAsync(string newUsername)
    {
        UsernameChangeRequest request = new UsernameChangeRequest
        {
            SessionToken = _session.SessionToken,
            NewUsername = InputText.Normalize(newUsername)
        };
        ProfileCall<UsernameChangeRequest, ProfileActionResult> usernameCall =
            new ProfileCall<UsernameChangeRequest, ProfileActionResult>(this, request, SendUsernameChangeAsync);
        Task<ProfileActionResult> changing = _callGuard.ExecuteAsync(usernameCall.SendAsync, _profileFailures);
        return changing;
    }

    public Task<ProfileActionResult> UpdateSocialLinksAsync(ProfileSocialLinks socialLinks)
    {
        if (socialLinks == null)
        {
            throw new ArgumentNullException(nameof(socialLinks));
        }

        SocialLinksUpdateRequest request = new SocialLinksUpdateRequest
        {
            SessionToken = _session.SessionToken,
            InstagramUsername = socialLinks.InstagramUsername,
            FacebookUsername = socialLinks.FacebookUsername,
            TwitterUsername = socialLinks.TwitterUsername
        };
        ProfileCall<SocialLinksUpdateRequest, ProfileActionResult> socialLinksCall =
            new ProfileCall<SocialLinksUpdateRequest, ProfileActionResult>(this, request, SendSocialLinksAsync);
        Task<ProfileActionResult> updating = _callGuard.ExecuteAsync(socialLinksCall.SendAsync, _profileFailures);
        return updating;
    }

    public Task<ProfileActionResult> UpdateAvatarAsync(byte[] imageBytes)
    {
        AvatarUpdateRequest request = new AvatarUpdateRequest
        {
            SessionToken = _session.SessionToken,
            ImageBytes = imageBytes
        };
        ProfileCall<AvatarUpdateRequest, ProfileActionResult> avatarCall =
            new ProfileCall<AvatarUpdateRequest, ProfileActionResult>(this, request, SendAvatarAsync);
        Task<ProfileActionResult> updating = _callGuard.ExecuteAsync(avatarCall.SendAsync, _profileFailures);
        return updating;
    }

    private static async Task<PlayerProfileResult> RequestProfileAsync(
        ProfileServiceClient profileClient,
        ProfileRequest request)
    {
        ProfileResponse response = await profileClient.GetProfileAsync(request).ConfigureAwait(false);
        PlayerProfileResult result = ProfileResultMapper.ToProfileResult(response);

        return result;
    }

    private static Task<ProfileActionResult> SendBiographyAsync(
        ProfileServiceClient profileClient,
        BiographyUpdateRequest request)
    {
        Task<ProfileActionResult> sending = RemoteCallGuard.CompleteCommandAsync(
            profileClient.UpdateBiographyAsync(request),
            ProfileActionResult.Success);
        return sending;
    }

    private static Task<ProfileActionResult> SendSocialLinksAsync(
        ProfileServiceClient profileClient,
        SocialLinksUpdateRequest request)
    {
        Task<ProfileActionResult> sending = RemoteCallGuard.CompleteCommandAsync(
            profileClient.UpdateSocialLinksAsync(request),
            ProfileActionResult.Success);
        return sending;
    }

    private static Task<ProfileActionResult> SendAvatarAsync(
        ProfileServiceClient profileClient,
        AvatarUpdateRequest request)
    {
        Task<ProfileActionResult> sending = RemoteCallGuard.CompleteCommandAsync(
            profileClient.UpdateAvatarAsync(request),
            ProfileActionResult.Success);
        return sending;
    }

    private async Task<ProfileActionResult> SendUsernameChangeAsync(
        ProfileServiceClient profileClient,
        UsernameChangeRequest request)
    {
        await profileClient.ChangeUsernameAsync(request).ConfigureAwait(false);
        _session.StoreUsername(request.NewUsername);

        return ProfileActionResult.Success;
    }

    private async Task<TResult> CallProfileServiceAsync<TRequest, TResult>(
        Func<ProfileServiceClient, TRequest, Task<TResult>> call,
        TRequest request)
    {
        ProfileServiceClient profileClient = _connectionFactory.CreateProfileClient();
        TResult result;

        try
        {
            result = await call(profileClient, request).ConfigureAwait(false);
        }
        finally
        {
            _channelCloser.Close(profileClient);
        }

        return result;
    }

    private sealed class ProfileCall<TRequest, TResult>
    {
        private readonly RemoteProfileServices _profileServices;
        private readonly TRequest _request;
        private readonly Func<ProfileServiceClient, TRequest, Task<TResult>> _call;

        public ProfileCall(
            RemoteProfileServices profileServices,
            TRequest request,
            Func<ProfileServiceClient, TRequest, Task<TResult>> call)
        {
            _profileServices = profileServices ?? throw new ArgumentNullException(nameof(profileServices));
            _request = request;
            _call = call ?? throw new ArgumentNullException(nameof(call));
        }

        public Task<TResult> SendAsync()
        {
            Task<TResult> sending = _profileServices.CallProfileServiceAsync(_call, _request);
            return sending;
        }
    }
}
