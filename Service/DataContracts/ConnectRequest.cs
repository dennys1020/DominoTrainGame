using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "ConnectRequest", Namespace = "http://dominotraingame/services")]
public sealed class ConnectRequest
{
    [DataMember]
    public string SessionToken { get; set; }
}
