using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "PlayerStatusCode", Namespace = "http://dominotraingame/services")]
public enum PlayerStatusCode
{
    [EnumMember]
    Offline,

    [EnumMember]
    Online,

    [EnumMember]
    InLobby,

    [EnumMember]
    InMatch
}
