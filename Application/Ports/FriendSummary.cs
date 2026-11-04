namespace DominoTrainGame.Server.Application.Ports;

public sealed class FriendSummary
{
    public FriendSummary(int playerId, string username)
    {
        PlayerId = playerId;
        Username = username;
    }

    public int PlayerId { get;  }
    public string Username { get;  }

}
