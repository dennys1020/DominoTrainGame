using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "LobbyInvitationMessage", Namespace = "http://dominotraingame/services")]
public sealed class LobbyInvitationMessage
{
    [DataMember]
    public string InviterUsername { get; set; }

    [DataMember]
    public string Code { get; set; }
}
