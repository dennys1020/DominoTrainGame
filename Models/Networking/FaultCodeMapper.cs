using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.Models.DominoService;

namespace DominoTrainGame.Client.Models.Networking;

internal static class FaultCodeMapper
{
    public static ConnectionCheckResult MapConnectionFault(GameServiceFaultCode faultCode)
    {
        ConnectionCheckResult connectionResult;

        switch (faultCode)
        {
            case GameServiceFaultCode.DatabaseUnavailable:
                connectionResult = ConnectionCheckResult.DatabaseUnavailable;
                break;
            default:
                connectionResult = ConnectionCheckResult.ServerError;
                break;
        }

        return connectionResult;
    }

    public static LoginResult MapLoginFault(GameServiceFaultCode faultCode)
    {
        LoginResult loginResult;

        switch (faultCode)
        {
            case GameServiceFaultCode.InvalidInput:
                loginResult = LoginResult.InvalidInput;
                break;
            case GameServiceFaultCode.DatabaseUnavailable:
                loginResult = LoginResult.DatabaseUnavailable;
                break;
            case GameServiceFaultCode.AlreadyConnected:
                loginResult = LoginResult.AlreadyConnected;
                break;
            default:
                loginResult = LoginResult.ServerError;
                break;
        }

        return loginResult;
    }

    public static SignUpResult MapSignUpFault(GameServiceFaultCode faultCode)
    {
        SignUpResult signUpResult;

        switch (faultCode)
        {
            case GameServiceFaultCode.InvalidInput:
                signUpResult = SignUpResult.InvalidInput;
                break;
            case GameServiceFaultCode.DatabaseUnavailable:
                signUpResult = SignUpResult.DatabaseUnavailable;
                break;
            case GameServiceFaultCode.EmailUnavailable:
                signUpResult = SignUpResult.EmailUnavailable;
                break;
            default:
                signUpResult = SignUpResult.ServerError;
                break;
        }

        return signUpResult;
    }

    public static EmailVerificationResult MapEmailVerificationFault(GameServiceFaultCode faultCode)
    {
        EmailVerificationResult verificationResult;

        switch (faultCode)
        {
            case GameServiceFaultCode.InvalidVerificationCode:
                verificationResult = EmailVerificationResult.InvalidCode;
                break;
            case GameServiceFaultCode.VerificationCodeExpired:
                verificationResult = EmailVerificationResult.CodeExpired;
                break;
            case GameServiceFaultCode.TooManyVerificationAttempts:
                verificationResult = EmailVerificationResult.TooManyAttempts;
                break;
            case GameServiceFaultCode.EmailUnavailable:
                verificationResult = EmailVerificationResult.EmailUnavailable;
                break;
            case GameServiceFaultCode.VerificationNotFound:
                verificationResult = EmailVerificationResult.RegistrationNotFound;
                break;
            case GameServiceFaultCode.ResendTooSoon:
                verificationResult = EmailVerificationResult.ResendTooSoon;
                break;
            case GameServiceFaultCode.AccountAlreadyExists:
                verificationResult = EmailVerificationResult.AlreadyExists;
                break;
            case GameServiceFaultCode.DatabaseUnavailable:
                verificationResult = EmailVerificationResult.DatabaseUnavailable;
                break;
            default:
                verificationResult = EmailVerificationResult.ServerError;
                break;
        }

        return verificationResult;
    }

    public static LobbyActionResult MapLobbyFault(GameServiceFaultCode faultCode)
    {
        LobbyActionResult resultStatus;

        switch (faultCode)
        {
            case GameServiceFaultCode.InvalidLobbyRequest:
                resultStatus = LobbyActionResult.InvalidCode;
                break;
            case GameServiceFaultCode.InvalidInput:
                resultStatus = LobbyActionResult.InvalidInput;
                break;
            case GameServiceFaultCode.LobbyNotFound:
                resultStatus = LobbyActionResult.LobbyNotFound;
                break;
            case GameServiceFaultCode.LobbyFull:
                resultStatus = LobbyActionResult.LobbyFull;
                break;
            case GameServiceFaultCode.LobbyConflict:
                resultStatus = LobbyActionResult.AlreadyInLobby;
                break;
            default:
                resultStatus = MapMatchSetupFault(faultCode);
                break;
        }

        return resultStatus;
    }

    public static MatchMoveResult MapMatchFault(GameServiceFaultCode faultCode)
    {
        MatchMoveResult resultStatus;

        switch (faultCode)
        {
            case GameServiceFaultCode.NotYourTurn:
                resultStatus = MatchMoveResult.NotYourTurn;
                break;
            case GameServiceFaultCode.TileDoesNotMatch:
                resultStatus = MatchMoveResult.TileDoesNotMatch;
                break;
            case GameServiceFaultCode.MatchNotFound:
                resultStatus = MatchMoveResult.MatchNotFound;
                break;
            case GameServiceFaultCode.TileNotInHand:
                resultStatus = MatchMoveResult.TileNotInHand;
                break;
            case GameServiceFaultCode.TrainNotAvailable:
                resultStatus = MatchMoveResult.TrainNotAvailable;
                break;
            case GameServiceFaultCode.MustSatisfyDouble:
                resultStatus = MatchMoveResult.MustSatisfyDouble;
                break;
            default:
                resultStatus = MapTurnFault(faultCode);
                break;
        }

        return resultStatus;
    }

    private static LobbyActionResult MapMatchSetupFault(GameServiceFaultCode faultCode)
    {
        LobbyActionResult resultStatus;

        switch (faultCode)
        {
            case GameServiceFaultCode.NotLobbyHost:
                resultStatus = LobbyActionResult.NotLobbyHost;
                break;
            case GameServiceFaultCode.PlayersNotReady:
                resultStatus = LobbyActionResult.PlayersNotReady;
                break;
            case GameServiceFaultCode.MatchAlreadyStarted:
                resultStatus = LobbyActionResult.MatchAlreadyStarted;
                break;
            case GameServiceFaultCode.DominoSetTooSmall:
                resultStatus = LobbyActionResult.DominoSetTooSmall;
                break;
            default:
                resultStatus = MapInvitationFault(faultCode);
                break;
        }

        return resultStatus;
    }

    private static LobbyActionResult MapInvitationFault(GameServiceFaultCode faultCode)
    {
        LobbyActionResult resultStatus;

        switch (faultCode)
        {
            case GameServiceFaultCode.PlayerNotFound:
                resultStatus = LobbyActionResult.PlayerNotFound;
                break;
            case GameServiceFaultCode.NotFriends:
                resultStatus = LobbyActionResult.NotFriends;
                break;
            case GameServiceFaultCode.PlayerOffline:
                resultStatus = LobbyActionResult.PlayerOffline;
                break;
            case GameServiceFaultCode.GuestNotAllowed:
                resultStatus = LobbyActionResult.GuestNotAllowed;
                break;
            default:
                resultStatus = MapSessionFault(faultCode);
                break;
        }

        return resultStatus;
    }

    private static LobbyActionResult MapSessionFault(GameServiceFaultCode faultCode)
    {
        LobbyActionResult resultStatus;

        switch (faultCode)
        {
            case GameServiceFaultCode.SessionNotFound:
            case GameServiceFaultCode.InvalidSessionToken:
                resultStatus = LobbyActionResult.SessionNotFound;
                break;
            case GameServiceFaultCode.AlreadyConnected:
                resultStatus = LobbyActionResult.AlreadyConnected;
                break;
            default:
                resultStatus = MapChatFault(faultCode);
                break;
        }

        return resultStatus;
    }

    private static LobbyActionResult MapChatFault(GameServiceFaultCode faultCode)
    {
        LobbyActionResult resultStatus;

        switch (faultCode)
        {
            case GameServiceFaultCode.ChatWarning:
                resultStatus = LobbyActionResult.ChatWarning;
                break;
            case GameServiceFaultCode.ChatBlocked:
                resultStatus = LobbyActionResult.ChatBlocked;
                break;
            case GameServiceFaultCode.PlayerBanned:
                resultStatus = LobbyActionResult.PlayerBanned;
                break;
            default:
                resultStatus = LobbyActionResult.ServerError;
                break;
        }

        return resultStatus;
    }

    public static LeaderboardStatus MapLeaderboardFault(GameServiceFaultCode faultCode)
    {
        LeaderboardStatus leaderboardStatus;

        switch (faultCode)
        {
            case GameServiceFaultCode.DatabaseUnavailable:
                leaderboardStatus = LeaderboardStatus.DatabaseUnavailable;
                break;
            case GameServiceFaultCode.SessionNotFound:
            case GameServiceFaultCode.InvalidSessionToken:
                leaderboardStatus = LeaderboardStatus.SessionNotFound;
                break;
            default:
                leaderboardStatus = LeaderboardStatus.ServerError;
                break;
        }

        return leaderboardStatus;
    }

    private static MatchMoveResult MapTurnFault(GameServiceFaultCode faultCode)
    {
        MatchMoveResult resultStatus;

        switch (faultCode)
        {
            case GameServiceFaultCode.BoneyardEmpty:
                resultStatus = MatchMoveResult.BoneyardEmpty;
                break;
            case GameServiceFaultCode.AlreadyDrawn:
                resultStatus = MatchMoveResult.AlreadyDrawn;
                break;
            case GameServiceFaultCode.MustDrawFirst:
                resultStatus = MatchMoveResult.MustDrawFirst;
                break;
            case GameServiceFaultCode.SessionNotFound:
            case GameServiceFaultCode.InvalidSessionToken:
                resultStatus = MatchMoveResult.SessionNotFound;
                break;
            default:
                resultStatus = MatchMoveResult.ServerError;
                break;
        }

        return resultStatus;
    }
}
