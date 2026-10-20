using System;
using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContractName(Name = "SignUpRequest", Namespace = "http://dominotraingame/services")]

public sealed class SignUpRequest
{
    [DataMember]

    public string UserName { get; set; }

    [DataMember]

    public string Email { get; set; }

    [DataMember]

    public string Password { get; set; }

}
