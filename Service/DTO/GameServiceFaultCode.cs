using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract(Name = "GameServiceFaultCode", Namespace = "http://dominotraingame/services")]
public enum GameServiceFaultCode
{
    [EnumMember]
    InvalidInput,

    [EnumMember]
    DatabaseUnavailable,

    [EnumMember]
    SessionNotFound,

    [EnumMember]
    LobbyNotFound,

    [EnumMember]
    LobbyFull,

    [EnumMember]
    LobbyConflict,

    [EnumMember]
    AlreadyConnected,

    [EnumMember]
    InvalidLobbyRequest,

    [EnumMember]
    InvalidSessionToken,

    [EnumMember]
    NotLobbyHost,

    [EnumMember]
    PlayersNotReady,
    
    [EnumMember]
    PlayerNotFound,

    [EnumMember]
    CannotTargetSelf,

    [EnumMember]
    AlreadyFriends,

    [EnumMember]
    FriendRequestAlreadyExists,

    [EnumMember]
    PlayerBlocked,

    [EnumMember]
    FriendRequestNotFound,

    [EnumMember]
    NotFriends
}
