using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract(Name = "SignUpRequest", Namespace = "http://dominotraingame/services")]
public sealed class SignUpRequest
{
    [DataMember]
    public string Username { get; set; }

    [DataMember]
    public string Email { get; set; }

    [DataMember]
    public string Password { get; set; }
}
