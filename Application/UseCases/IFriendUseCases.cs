using System.Collections.Generic;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.UseCases;

public interface IFriendUseCases
{
    IReadOnlyList<FriendSummary> GetFriends(int playerId);

    IReadOnlyList<FriendRequestSummary> GetPendingRequests(int playerId);

    IReadOnlyList<FriendSummary> SearchPlayers(int playerId, string usernameFragment);

    SendFriendRequestStatus SendFriendRequest(int requesterPlayerId, string addresseeUsername);

    bool AcceptFriendRequest(int playerId, int requestId);

    bool RejectFriendRequest(int playerId, int requestId);

    RemoveFriendStatus RemoveFriend(int playerId, string friendUsername);
}
