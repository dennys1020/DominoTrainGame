using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "BiographyUpdateRequest", Namespace = "http://dominotraingame/services")]
public sealed class BiographyUpdateRequest
{
    [DataMember]
    public string SessionToken { get; set; }

    [DataMember]
    public string Biography { get; set; }
}
