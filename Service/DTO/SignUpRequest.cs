using System;
using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Servic.DTO

[DataContractName(Name = "SignUpRequest", Namespace = "http://dominotraingame/services"]

public sealed class SignUpRequest
{
    [DataMember]

    public Guid RequestId { get; set; }

    [DataMember]

    public string UserName { get; set; }

    [DataMember]

    public string Email { get; set; }

    [DataMember]

    public string password { get; set; }


    }
}
