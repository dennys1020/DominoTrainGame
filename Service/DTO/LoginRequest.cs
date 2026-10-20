using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract(Name = "LoginRequest", Namespace = "http://dominotraingame/services")]
public sealed class LoginRequest
{
    [DataMember]
    public string Identifier { get; set; }

    [DataMember]
    public string Password { get; set; }
}
