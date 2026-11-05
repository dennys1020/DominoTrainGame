using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "PlayerSearchRequest", Namespace = "http://dominotraingame/services")]
public sealed class PlayerSearchRequest
{
    [DataMember]
    public string UsernameFragment { get; set; }
}
