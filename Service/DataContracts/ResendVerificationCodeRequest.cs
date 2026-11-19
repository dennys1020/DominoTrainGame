using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "ResendVerificationCodeRequest", Namespace = "http://dominotraingame/services")]
public sealed class ResendVerificationCodeRequest
{
    [DataMember]
    public string Email { get; set; }

    [DataMember(IsRequired = false)]
    public string Language { get; set; }
}
