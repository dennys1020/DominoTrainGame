using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DTO;

[DataContract]
public enum LoginValidationStatus
{
    [EnumMember]
    Success,
    [EnumMember]
    EmptyFields,
    [EnumMember]
    UserNotFound,
    [EnumMember]
    IncorrectPassword,
    [EnumMember]
    DatabaseError
}
