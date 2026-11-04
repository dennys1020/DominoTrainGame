using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract(Name = "PlayerSearchResponse", Namespace = "http://dominotraingame/services")]
public sealed class PlayerSearchResponse
{
    [DataMember]
    public int PlayerId { get; set; }

    [DataMember]
    public string Username { get; set; }
}
