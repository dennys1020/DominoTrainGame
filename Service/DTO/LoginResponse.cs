using System;
using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract(Name = "LoginResponse", Namespace = "http://dominotraingame/services")]
public sealed class LoginResponse
{
    [DataMember]
    public Guid RequestId { get; set; }

    [DataMember]
    public LoginStatus Status { get; set; }
}
