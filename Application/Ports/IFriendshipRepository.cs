using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Application.Ports;

public interface IFriendshipRepository
{
    PlayerIdentity FindRegisteredPlayer(string username);
    IReadOnlyList<PlayerIdentity> GetFriends(int playerId);
    IReadOnlyList<FriendRequestSummary> GetPendingRequests(int playerId);
    IReadOnlyList<PlayerIdentity> SearchRegisteredPlayers(int playerId, string usernameFragment);
    FriendshipState GetFriendshipState(int playerId, int friendPlayerId);
    bool CreateRequest(int requestPlayerId, int targetPlayerId);
    IReadOnlyList<int> AcceptRequest(int requestId, int targetPlayerId);
    bool RejectRequest(int requestId, int targetPlayerId);
    bool RemoveFriendship(int playerId, int friendPlayerId);
}
