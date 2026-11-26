using System.ServiceModel;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Guards;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.UseCases;
using DominoTrainGame.Server.Service.Contracts;
using DominoTrainGame.Server.Service.DataContracts;
using DominoTrainGame.Server.Service.Faults;
using DominoTrainGame.Server.Service.Sessions;
using DominoTrainGame.Shared.Common;

namespace DominoTrainGame.Server.Service.GameServices;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
public sealed class ProfileService : IProfileService
{
    private static readonly ITraceLog _logger;

    private readonly IProfileUseCases _profileUseCases;
    private readonly SessionTokenRegistry _sessionTokens;
    private readonly ServiceFaultTranslator _faultTranslator;

    static ProfileService()
    {
        _logger = TraceLogManager.GetLogger(typeof(ProfileService));
    }

    public ProfileService(
        IProfileUseCases profileUseCases,
        SessionTokenRegistry sessionTokenRegistry,
        ServiceFaultTranslator faultTranslator)
    {
        _profileUseCases = ArgumentGuard.NotNull(profileUseCases, nameof(profileUseCases), _logger);
        _sessionTokens = ArgumentGuard.NotNull(sessionTokenRegistry, nameof(sessionTokenRegistry), _logger);
        _faultTranslator = ArgumentGuard.NotNull(faultTranslator, nameof(faultTranslator), _logger);
    }

    public ProfileResponse GetProfile(ProfileRequest profileRequest)
    {
        string username = profileRequest?.Username;
        TokenCallerResolver.ResolveRegisteredCaller(_sessionTokens, profileRequest?.SessionToken);
        PlayerProfileSummary profile = _faultTranslator.Execute(() => _profileUseCases.FindProfileByUsername(username));

        GameFaultFactory.ThrowIfRejected(!profile.IsEmpty, GameServiceFaultCode.PlayerNotFound);

        PlayerProfileStatistics statistics = _faultTranslator.Execute(
            () => _profileUseCases.GetProfileStatistics(profile.Player.PlayerId));
        byte[] avatarImage = _faultTranslator.Execute(() => _profileUseCases.LoadProfileAvatarImage(profile));
        ProfileResponse response = ToProfileResponse(profile, statistics, avatarImage);

        return response;
    }

    public void UpdateBiography(BiographyUpdateRequest biographyRequest)
    {
        string biography = biographyRequest?.Biography;
        PlayerIdentity caller = TokenCallerResolver.ResolveRegisteredCaller(
            _sessionTokens,
            biographyRequest?.SessionToken);
        bool isUpdated = _faultTranslator.Execute(
            () => _profileUseCases.UpdateProfileBiography(caller.PlayerId, biography));

        GameFaultFactory.ThrowIfRejected(isUpdated, GameServiceFaultCode.GuestNotAllowed);
    }

    public void ChangeUsername(UsernameChangeRequest usernameRequest)
    {
        string newUsername = InputText.Normalize(usernameRequest?.NewUsername);
        PlayerIdentity caller = TokenCallerResolver.ResolveRegisteredCaller(
            _sessionTokens,
            usernameRequest?.SessionToken);
        ChangeUsernameStatus status = _faultTranslator.Execute(
            () => _profileUseCases.ChangeProfileUsername(caller.PlayerId, newUsername));

        GameFaultFactory.ThrowIfRejected(status, ChangeUsernameStatus.Success, MapChangeUsernameFaultCode);
        _sessionTokens.UpdateUsername(caller.PlayerId, newUsername);
    }

    public void UpdateSocialLinks(SocialLinksUpdateRequest socialLinksRequest)
    {
        PlayerIdentity caller = TokenCallerResolver.ResolveRegisteredCaller(
            _sessionTokens,
            socialLinksRequest?.SessionToken);
        SocialLinks socialLinks = new SocialLinks(
            socialLinksRequest?.InstagramUsername,
            socialLinksRequest?.FacebookUsername,
            socialLinksRequest?.TwitterUsername);
        bool isUpdated = _faultTranslator.Execute(
            () => _profileUseCases.UpdateProfileSocialLinks(caller.PlayerId, socialLinks));

        GameFaultFactory.ThrowIfRejected(isUpdated, GameServiceFaultCode.GuestNotAllowed);
    }

    public void UpdateAvatar(AvatarUpdateRequest avatarRequest)
    {
        byte[] imageBytes = avatarRequest?.ImageBytes;
        PlayerIdentity caller = TokenCallerResolver.ResolveRegisteredCaller(
            _sessionTokens,
            avatarRequest?.SessionToken);
        bool isUpdated = _faultTranslator.Execute(
            () => _profileUseCases.UpdateProfileAvatar(caller.PlayerId, imageBytes));

        GameFaultFactory.ThrowIfRejected(isUpdated, GameServiceFaultCode.GuestNotAllowed);
    }

    private static ProfileResponse ToProfileResponse(
        PlayerProfileSummary profile,
        PlayerProfileStatistics statistics,
        byte[] avatarImage)
    {
        ProfileResponse response = new ProfileResponse
        {
            Username = profile.Player.Username,
            Biography = profile.Details.Biography,
            AvatarImage = avatarImage,
            InstagramUsername = profile.SocialLinks.InstagramUsername,
            FacebookUsername = profile.SocialLinks.FacebookUsername,
            TwitterUsername = profile.SocialLinks.TwitterUsername,
            MemberSince = profile.Details.MemberSince,
            GamesPlayed = statistics.GamesPlayed,
            Wins = statistics.Wins,
            TotalPoints = statistics.TotalPoints
        };

        return response;
    }

    private static GameServiceFaultCode MapChangeUsernameFaultCode(ChangeUsernameStatus status)
    {
        GameServiceFaultCode faultCode;

        switch (status)
        {
            case ChangeUsernameStatus.UsernameTaken:
                faultCode = GameServiceFaultCode.UsernameAlreadyTaken;
                break;
            case ChangeUsernameStatus.GuestNotAllowed:
                faultCode = GameServiceFaultCode.GuestNotAllowed;
                break;
            default:
                faultCode = GameServiceFaultCode.InvalidInput;
                break;
        }

        return faultCode;
    }
}
