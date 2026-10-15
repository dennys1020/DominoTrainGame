using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public sealed class FindAccountRequest
{
    [DataMember]
    public string Identifier { get; set; } = string.Empty;
}
