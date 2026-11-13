using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "SessionTokenRequest", Namespace = "http://dominotraingame/services")]
public sealed class SessionTokenRequest
{
    [DataMember]
    public string SessionToken { get; set; }
}
