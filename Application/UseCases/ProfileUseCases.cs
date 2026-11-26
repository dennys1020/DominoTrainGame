using System;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Guards;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.UseCases.Validation;

namespace DominoTrainGame.Server.Application.UseCases;

public sealed class ProfileUseCases : IProfileUseCases
{
    private const string BiographyFieldName = "biography";
    private const string SocialLinksFieldName = "social links";
    private const string AvatarFieldName = "avatar";

    private static readonly ITraceLog _logger;

    private readonly IMatchResultRepository _matchResultRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly IAvatarStorage _avatarStorage;

    static ProfileUseCases()
    {
        _logger = TraceLogManager.GetLogger(typeof(ProfileUseCases));
    }

    public ProfileUseCases(
        IProfileRepository profileRepository,
        IMatchResultRepository matchResultRepository,
        IAvatarStorage avatarStorage)
    {
        _matchResultRepository = ArgumentGuard.NotNull(matchResultRepository, nameof(matchResultRepository), _logger);
        _profileRepository = ArgumentGuard.NotNull(profileRepository, nameof(profileRepository), _logger);
        _avatarStorage = ArgumentGuard.NotNull(avatarStorage, nameof(avatarStorage), _logger);
    }

    public PlayerProfileSummary FindProfileByUsername(string username)
    {
        string normalizadedUsername = PlayerInputValidator.NormalizeUsername(username);
        PlayerProfileSummary profile = _profileRepository.FindProfileByUsername(normalizadedUsername);

        return profile;
    }

    public PlayerProfileStatistics GetProfileStatistics(int playerId)
    {
        PlayerProfileStatistics profileStatistics = _matchResultRepository.GetProfileStatisticsByPlayerId(playerId);

        return profileStatistics;
    }

    public byte[] LoadProfileAvatarImage(PlayerProfileSummary playerProfile)
    {
        ArgumentGuard.ThrowIfFull(playerProfile, nameof(playerProfile), _logger);
        byte[] avatarImage = Array.Empty<byte>();

        if (playerProfile.Details.HasAvatar)
        {
            avatarImage = LoadStoredAvatarImage(playerProfile.Details.AvatarFileName);
        }

        return avatarImage;
    }

    public bool UpdateProfileBiography(int playerId, string biography)
    {
        string normalizedBiography = ProfileInputValidator.NormalizeBiography(biography);
        bool isUpdateProfile = _profileRepository.UpdateBiography(playerId, normalizedBiography);

        LogProfileUpdate(BiographyFieldName, playerId, isUpdateProfile);

        return isUpdateProfile;
    }

    public ChangeUsernameStatus ChangeProfileUsername(int playerId, string newUsername)
    {
        string normalizadedUsername = ProfileInputValidator.NormalizeNewUsername(newUsername);
        ChangeUsernameStatus usernameStatus = SaveNewUsername(playerId, normalizadedUsername);

        LogUsernameChange(playerId, usernameStatus);

        return usernameStatus;
    }

    public bool UpdateProfileSocialLinks(int playerId, SocialLinks socialLinks)
    {
        SocialLinks normalizadedSocialLinks = ProfileInputValidator.NormalizeSocialLinks(socialLinks);
        bool isUpdateSocialLinks = _profileRepository.UpdateSocialLinks(playerId, normalizadedSocialLinks);

        LogProfileUpdate(SocialLinksFieldName, playerId, isUpdateSocialLinks);

        return isUpdateSocialLinks;
    }

    public bool UpdateProfileAvatar(int playerId, byte[] imageBytes)
    {
        string fileExtension = AvatarImageValidator.ResolveAvatarFileExtension(imageBytes);
        PlayerProfileSummary playerProfile = _profileRepository.FindProfileByPlayerId(playerId);
        bool isUpdateAvatar = !playerProfile.IsEmpty && ReplaceAvatarImage(playerProfile, imageBytes, fileExtension);

        LogProfileUpdate(AvatarFieldName, playerId, isUpdateAvatar);

        return isUpdateAvatar;
    }

    private static void LogProfileUpdate(string fieldName, int playerId, bool isUpdated)
    {
        if (isUpdated)
        {
            _logger.InfoFormat("A profile {0} was updated. PlayerId: {1}.", fieldName, playerId);
        }
        else
        {
            _logger.InfoFormat(
                "A profile {0} update was rejected because the player has no profile. PlayerId: {1}.",
                fieldName,
                playerId);
        }
    }

    private static void LogUsernameChange(int playerId, ChangeUsernameStatus status)
    {
        if (status == ChangeUsernameStatus.Success)
        {
            _logger.InfoFormat("A username was changed. PlayerId: {0}.", playerId);
        }
        else
        {
            _logger.InfoFormat("A username change was rejected. PlayerId: {0}. Status: {1}.", playerId, status);
        }
    }

    private ChangeUsernameStatus SaveNewUsername(int playerId, string normalizedUsername)
    {
        ChangeUsernameStatus status;

        try
        {
            status = _profileRepository.ChangeUsername(playerId, normalizedUsername);
        }
        catch (DataIntegrityException exception)
        {
            _logger.Info("A username change lost the race for the same name against another player.", exception);
            status = ChangeUsernameStatus.UsernameTaken;
        }

        return status;
    }

    private byte[] LoadStoredAvatarImage(string avatarFileName)
    {
        byte[] avatarImage;

        try
        {
            avatarImage = _avatarStorage.LoadAvatarImage(avatarFileName);
        }
        catch (AvatarStorageUnavailableException exception)
        {
            _logger.Warn("A profile was returned without avatar because the image could not be loaded.", exception);
            avatarImage = Array.Empty<byte>();
        }

        return avatarImage;
    }

    private bool ReplaceAvatarImage(PlayerProfileSummary profile, byte[] imageBytes, string fileExtension)
    {
        string newAvatarFileName = _avatarStorage.SaveAvatarImage(imageBytes, fileExtension);
        bool isSaved = SaveAvatarFileName(profile.Player.PlayerId, newAvatarFileName);
        string discardedAvatarFileName = isSaved ? profile.Details.AvatarFileName : newAvatarFileName;

        DeleteDiscardedAvatarImage(discardedAvatarFileName);
        return isSaved;
    }

    private bool SaveAvatarFileName(int playerId, string newAvatarFileName)
    {
        bool isSaved;

        try
        {
            isSaved = _profileRepository.UpdateAvatarFileName(playerId, newAvatarFileName);
        }
        catch (DatabaseUnavailableException)
        {
            DeleteDiscardedAvatarImage(newAvatarFileName);
            throw;
        }
        catch (DataIntegrityException)
        {
            DeleteDiscardedAvatarImage(newAvatarFileName);
            throw;
        }

        return isSaved;
    }

    private void DeleteDiscardedAvatarImage(string avatarFileName)
    {
        bool hasFileToDelete = !string.IsNullOrEmpty(avatarFileName);

        if (hasFileToDelete)
        {
            try
            {
                _avatarStorage.DeleteAvatarImage(avatarFileName);
            }
            catch (AvatarStorageUnavailableException exception)
            {
                _logger.Warn("A discarded avatar image could not be deleted from the avatar folder.", exception);
            }
        }
    }
}
