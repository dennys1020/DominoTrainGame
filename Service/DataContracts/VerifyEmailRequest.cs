using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "VerifyEmailRequest", Namespace = "http://dominotraingame/services")]
public sealed class VerifyEmailRequest
{
    [DataMember]
    public string Email { get; set; }

    [DataMember]
    public string VerificationCode { get; set; }
}
