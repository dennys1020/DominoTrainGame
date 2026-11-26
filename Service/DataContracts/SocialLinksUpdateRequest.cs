using System.Runtime.Serialization;

namespace DominoTrainGame.Server.Service.DataContracts;

[DataContract(Name = "SocialLinksUpdateRequest", Namespace = "http://dominotraingame/services")]
public sealed class SocialLinksUpdateRequest
{
    [DataMember]
    public string SessionToken { get; set; }

    [DataMember]
    public string InstagramUsername { get; set; }

    [DataMember]
    public string FacebookUsername { get; set; }

    [DataMember]
    public string TwitterUsername { get; set; }
}
