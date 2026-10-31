using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract(Name = "LoginResponse", Namespace = "http://dominotraingame/services")]
public sealed class LoginResponse
{
    [DataMember]
    public bool IsAuthenticated { get; set; }

    [DataMember]
    public int PlayerId { get; set; }

    [DataMember]
    public string Username { get; set; }

    [DataMember]
    public string SessionToken { get; set; }
}
