using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "ProfileRequest", Namespace = "http://dominotraingame/services")]
public sealed class ProfileRequest
{
    [DataMember]
    public string SessionToken { get; set; }

    [DataMember]
    public string Username { get; set; }
}
