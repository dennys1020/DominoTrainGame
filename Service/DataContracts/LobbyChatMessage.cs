using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "LobbyChatMessage", Namespace = "http://dominotraingame/services")]
public sealed class LobbyChatMessage
{
    [DataMember]
    public int PlayerId { get; set; }

    [DataMember]
    public string Username { get; set; }

    [DataMember]
    public string Text { get; set; }
}
