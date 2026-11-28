using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Networking;

internal static class ProfileResultMapper
{
    public static PlayerProfileResult ToProfileResult(ProfileResponse response)
    {
        PlayerProfileResult result = PlayerProfileResult.CreateFailure(ProfileActionResult.ServerError);

        if (response != null)
        {
            result = new PlayerProfileResult(ProfileActionResult.Success, ToPlayerProfile(response));
        }

        return result;
    }

    public static ProfileActionResult MapFault(GameServiceFaultCode faultCode)
    {
        ProfileActionResult resultStatus;

        switch (faultCode)
        {
            case GameServiceFaultCode.PlayerNotFound:
                resultStatus = ProfileActionResult.PlayerNotFound;
                break;
            case GameServiceFaultCode.UsernameAlreadyTaken:
                resultStatus = ProfileActionResult.UsernameAlreadyTaken;
                break;
            case GameServiceFaultCode.GuestNotAllowed:
                resultStatus = ProfileActionResult.GuestNotAllowed;
                break;
            case GameServiceFaultCode.AvatarStorageUnavailable:
                resultStatus = ProfileActionResult.AvatarStorageUnavailable;
                break;
            default:
                resultStatus = MapRequestFault(faultCode);
                break;
        }

        return resultStatus;
    }

    private static ProfileActionResult MapRequestFault(GameServiceFaultCode faultCode)
    {
        ProfileActionResult resultStatus;

        switch (faultCode)
        {
            case GameServiceFaultCode.InvalidInput:
                resultStatus = ProfileActionResult.InvalidInput;
                break;
            case GameServiceFaultCode.DatabaseUnavailable:
                resultStatus = ProfileActionResult.DatabaseUnavailable;
                break;
            case GameServiceFaultCode.SessionNotFound:
            case GameServiceFaultCode.InvalidSessionToken:
                resultStatus = ProfileActionResult.SessionNotFound;
                break;
            default:
                resultStatus = ProfileActionResult.ServerError;
                break;
        }

        return resultStatus;
    }

    private static PlayerProfile ToPlayerProfile(ProfileResponse response)
    {
        ProfileSocialLinks socialLinks = new ProfileSocialLinks(
            response.InstagramUsername,
            response.FacebookUsername,
            response.TwitterUsername);
        ProfileStatistics statistics = new ProfileStatistics(
            response.GamesPlayed,
            response.Wins,
            response.TotalPoints);
        PlayerProfile profile = new PlayerProfile(
            response.Username,
            response.Biography,
            response.AvatarImage,
            response.MemberSince,
            socialLinks,
            statistics);

        return profile;
    }
}
