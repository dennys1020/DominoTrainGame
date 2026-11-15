using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "PlayerScoreData", Namespace =
    "http://dominotraingame/services")]
public sealed class PlayerScoreData
{
    [DataMember]
    public int PlayerId { get; set; }
    [DataMember]
    public int Score { get; set; }
}
