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
            default:
                signUpResult = SignUpResult.ServerError;
                break;
        }

        return signUpResult;
    }

    public static LobbyResultStatus MapLobbyFault(GameServiceFaultCode faultCode)
    {
        LobbyResultStatus resultStatus;

        switch (faultCode)
        {
            case GameServiceFaultCode.InvalidLobbyRequest:
                resultStatus = LobbyResultStatus.InvalidCode;
                break;
            case GameServiceFaultCode.InvalidInput:
                resultStatus = LobbyResultStatus.InvalidInput;
                break;
            case GameServiceFaultCode.LobbyNotFound:
                resultStatus = LobbyResultStatus.LobbyNotFound;
                break;
            case GameServiceFaultCode.LobbyFull:
                resultStatus = LobbyResultStatus.LobbyFull;
                break;
            case GameServiceFaultCode.LobbyConflict:
                resultStatus = LobbyResultStatus.AlreadyInLobby;
                break;
            case GameServiceFaultCode.NotLobbyHost:
                resultStatus = LobbyResultStatus.NotLobbyHost;
                break;
            case GameServiceFaultCode.PlayersNotReady:
                resultStatus = LobbyResultStatus.PlayersNotReady;
                break;
            default:
                resultStatus = MapInvitationFault(faultCode);
                break;
        }

        return resultStatus;
    }

    public static MatchResultStatus MapMatchFault(GameServiceFaultCode faultCode)
    {
        MatchResultStatus resultStatus;

        switch (faultCode)
        {
            case GameServiceFaultCode.NotYourTurn:
                resultStatus = MatchResultStatus.NotYourTurn;
                break;
            case GameServiceFaultCode.TileDoesNotMatch:
                resultStatus = MatchResultStatus.TileDoesNotMatch;
                break;
            case GameServiceFaultCode.MatchNotFound:
                resultStatus = MatchResultStatus.MatchNotFound;
                break;
            case GameServiceFaultCode.TileNotInHand:
                resultStatus = MatchResultStatus.TileNotInHand;
                break;
            case GameServiceFaultCode.TrainNotAvailable:
                resultStatus = MatchResultStatus.TrainNotAvailable;
                break;
            default:
                resultStatus = MapTurnFault(faultCode);
                break;
        }

        return resultStatus;
    }

    private static LobbyResultStatus MapInvitationFault(GameServiceFaultCode faultCode)
    {
        LobbyResultStatus resultStatus;

        switch (faultCode)
        {
            case GameServiceFaultCode.PlayerNotFound:
                resultStatus = LobbyResultStatus.PlayerNotFound;
                break;
            case GameServiceFaultCode.NotFriends:
                resultStatus = LobbyResultStatus.NotFriends;
                break;
            case GameServiceFaultCode.PlayerOffline:
                resultStatus = LobbyResultStatus.PlayerOffline;
                break;
            default:
                resultStatus = MapSessionFault(faultCode);
                break;
        }

        return resultStatus;
    }

    private static LobbyResultStatus MapSessionFault(GameServiceFaultCode faultCode)
    {
        LobbyResultStatus resultStatus;

        switch (faultCode)
        {
            case GameServiceFaultCode.SessionNotFound:
            case GameServiceFaultCode.InvalidSessionToken:
                resultStatus = LobbyResultStatus.SessionNotFound;
                break;
            case GameServiceFaultCode.AlreadyConnected:
                resultStatus = LobbyResultStatus.AlreadyConnected;
                break;
            default:
                resultStatus = MapChatFault(faultCode);
                break;
        }

        return resultStatus;
    }

    private static LobbyResultStatus MapChatFault(GameServiceFaultCode faultCode)
    {
        LobbyResultStatus resultStatus;

        switch (faultCode)
        {
            case GameServiceFaultCode.ChatWarning:
                resultStatus = LobbyResultStatus.ChatWarning;
                break;
            case GameServiceFaultCode.ChatBlocked:
                resultStatus = LobbyResultStatus.ChatBlocked;
                break;
            case GameServiceFaultCode.PlayerBanned:
                resultStatus = LobbyResultStatus.PlayerBanned;
                break;
            default:
                resultStatus = LobbyResultStatus.ServerError;
                break;
        }

        return resultStatus;
    }

    private static MatchResultStatus MapTurnFault(GameServiceFaultCode faultCode)
    {
        MatchResultStatus resultStatus;

        switch (faultCode)
        {
            case GameServiceFaultCode.BoneyardEmpty:
                resultStatus = MatchResultStatus.BoneyardEmpty;
                break;
            case GameServiceFaultCode.AlreadyDrawn:
                resultStatus = MatchResultStatus.AlreadyDrawn;
                break;
            case GameServiceFaultCode.MustDrawFirst:
                resultStatus = MatchResultStatus.MustDrawFirst;
                break;
            case GameServiceFaultCode.SessionNotFound:
            case GameServiceFaultCode.InvalidSessionToken:
                resultStatus = MatchResultStatus.SessionNotFound;
                break;
            default:
                resultStatus = MatchResultStatus.ServerError;
                break;
        }

        return resultStatus;
    }
}
