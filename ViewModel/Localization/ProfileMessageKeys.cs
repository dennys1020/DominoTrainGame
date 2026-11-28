using DominoTrainGame.Client.Models.Contracts;

namespace DominoTrainGame.Client.ViewModel.Localization;

public static class ProfileMessageKeys
{
    private const string ProfileInvalidInputMessageKey = "ProfileInvalidInputMessage";
    private const string ProfileUsernameTakenMessageKey = "ProfileUsernameTakenMessage";
    private const string ProfilePlayerNotFoundMessageKey = "ProfilePlayerNotFoundMessage";
    private const string ProfileGuestNotAllowedMessageKey = "ProfileGuestNotAllowedMessage";
    private const string ProfileAvatarStorageMessageKey = "ProfileAvatarStorageUnavailableMessage";
    private const string ProfileAvatarInvalidMessageKey = "ProfileAvatarInvalidMessage";
    private const string ProfileServerErrorMessageKey = "ProfileServerErrorMessage";

    public static string GetMessageKey(ProfileActionResult status)
    {
        string messageKey;

        switch (status)
        {
            case ProfileActionResult.InvalidInput:
                messageKey = ProfileInvalidInputMessageKey;
                break;
            case ProfileActionResult.UsernameAlreadyTaken:
                messageKey = ProfileUsernameTakenMessageKey;
                break;
            case ProfileActionResult.PlayerNotFound:
                messageKey = ProfilePlayerNotFoundMessageKey;
                break;
            case ProfileActionResult.GuestNotAllowed:
                messageKey = ProfileGuestNotAllowedMessageKey;
                break;
            default:
                messageKey = GetConnectionMessageKey(status);
                break;
        }

        return messageKey;
    }

    public static string GetAvatarMessageKey(ProfileActionResult status)
    {
        bool isImageRejected = status == ProfileActionResult.InvalidInput;
        string messageKey = isImageRejected ? ProfileAvatarInvalidMessageKey : GetMessageKey(status);

        return messageKey;
    }

    private static string GetConnectionMessageKey(ProfileActionResult status)
    {
        string messageKey;

        switch (status)
        {
            case ProfileActionResult.SessionNotFound:
                messageKey = LocalizationViewModel.SessionExpiredMessageKey;
                break;
            case ProfileActionResult.DatabaseUnavailable:
                messageKey = LocalizationViewModel.DatabaseErrorMessageKey;
                break;
            case ProfileActionResult.AvatarStorageUnavailable:
                messageKey = ProfileAvatarStorageMessageKey;
                break;
            case ProfileActionResult.ServerUnavailable:
                messageKey = LocalizationViewModel.ServerUnavailableMessageKey;
                break;
            default:
                messageKey = ProfileServerErrorMessageKey;
                break;
        }

        return messageKey;
    }
}
