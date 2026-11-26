using System.ServiceModel;
using DominoTrainGame.Server.Service.DataContracts;

namespace DominoTrainGame.Server.Service.Contracts;

/// <summary>
/// Receives the friend events that the server sends to the client.
/// </summary>
/// <remarks>
/// Every operation is one-way, so the server does not wait for the client.
/// </remarks>
[ServiceContract]
public interface ISocialClientCallback
{
    /// <summary>
    /// Receives the new status of a friend.
    /// </summary>
    /// <param name="message">The friend and the new status.</param>
    [OperationContract(IsOneWay = true)]
    void ReceiveFriendStatusChanged(FriendStatusMessage message);

    /// <summary>
    /// Receives the notice that the friend list or the pending requests changed and must be loaded again.
    /// </summary>
    [OperationContract(IsOneWay = true)]
    void ReceiveFriendsChanged();
}
