using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "LobbyResponse", Namespace = "http://dominotraingame/services")]
public sealed class LobbyResponse
{
    [DataMember]
    public string Code { get; set; }

    [DataMember]
    public int MaximumPlayers { get; set; }

    [DataMember]
    public long Version { get; set; }

    [DataMember]
    public int HostPlayerId { get; set; }

    [DataMember]
    public List<LobbyPlayer> Players { get; set; }
}
