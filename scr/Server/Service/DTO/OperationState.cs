using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public enum OperationState
{
    [EnumMember]
    Started,
    [EnumMember]
    Completed,
    [EnumMember]
    Failed
}
