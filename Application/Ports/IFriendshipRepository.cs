using System.Collections.Generic;

namespace DominoTrainGame.Server.Application.Ports;

public interface IFriendshipRepository
{
    bool TryFindFriendship(string username, out int playerId);
    IReadOnlyList<FriendSummary> GetFriends(int playerId);
    IReadOnlyList<FriendRequestSummary> GetPendingRequests(int playerId);
    FriendshipState GetFriendshipState(int playerId, int friendPlayerId);
    bool CreateRequest(int requestPlayerId, int targetPlayerId);
    bool AcceptRequest(int requestId, int targetPlayerId);
    bool RejectRequest(int requestId, int targetPlayerId);
    bool RemoveFriendship(int playerId, int friendPlayerId);
}
