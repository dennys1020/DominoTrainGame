using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "SetReadyRequest", Namespace = "http://dominotraingame/services")]
public sealed class SetReadyRequest
{
    [DataMember]
    public bool IsReady { get; set; }
}
