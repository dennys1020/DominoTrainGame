using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "AvatarUpdateRequest", Namespace = "http://dominotraingame/services")]
public sealed class AvatarUpdateRequest
{
    [DataMember]
    public string SessionToken { get; set; }

    [DataMember]
    public byte[] ImageBytes { get; set; }
}
