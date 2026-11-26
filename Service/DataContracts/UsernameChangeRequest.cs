using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "UsernameChangeRequest", Namespace = "http://dominotraingame/services")]
public sealed class UsernameChangeRequest
{
    [DataMember]
    public string SessionToken { get; set; }

    [DataMember]
    public string NewUsername { get; set; }
}
