using System.ServiceModel;
using DominoTrainGame.Server.Service.DataContracts;

namespace DominoTrainGame.Server.Service.Contracts;

/// <summary>
/// Receives the lobby and match events that the server sends to the client.
/// </summary>
/// <remarks>
/// Every operation is one-way, so the server does not wait for the client. A lobby event with a roster
/// version older than the one the client has must be ignored.
/// </remarks>
[ServiceContract(
    Name = "IGameClientCallback",
    Namespace = "http://tempuri.org/")]
public interface IGameClientCallback
{
    /// <summary>
    /// Receives a player that joined the lobby.
    /// </summary>
    /// <param name="message">The new member and the roster version.</param>
    [OperationContract(
        Action = "http://tempuri.org/IGameClientCallback/ReceivePlayerJoined",
        IsOneWay = true)]
    void ReceivePlayerJoined(PlayerJoinedMessage message);

    /// <summary>
    /// Receives a player that left the lobby.
    /// </summary>
    /// <param name="message">The player that left, the host and the roster version.</param>
    [OperationContract(
        Action = "http://tempuri.org/IGameClientCallback/ReceivePlayerLeft",
        IsOneWay = true)]
    void ReceivePlayerLeft(PlayerLeftMessage message);

    /// <summary>
    /// Receives the new ready state of a member of the lobby.
    /// </summary>
    /// <param name="message">The player, the ready state and the roster version.</param>
    [OperationContract(
        Action = "http://tempuri.org/IGameClientCallback/ReceivePlayerReadyChanged",
        IsOneWay = true)]
    void ReceivePlayerReadyChanged(PlayerReadyChangedMessage message);

    /// <summary>
    /// Receives the domino set that the host selected.
    /// </summary>
    /// <param name="message">The highest double of the set and the roster version.</param>
    [OperationContract(
        Action = "http://tempuri.org/IGameClientCallback/ReceiveDominoSetChanged",
        IsOneWay = true)]
    void ReceiveDominoSetChanged(DominoSetChangedMessage message);

    /// <summary>
    /// Receives a chat message of the lobby, with the offensive words already censored.
    /// </summary>
    /// <param name="message">The sender and the filtered text.</param>
    [OperationContract(
        Action = "http://tempuri.org/IGameClientCallback/ReceiveLobbyMessage",
        IsOneWay = true)]
    void ReceiveLobbyMessage(LobbyChatMessage message);

    /// <summary>
    /// Receives the notice that the host started the match.
    /// </summary>
    /// <param name="message">The code of the match.</param>
    [OperationContract(
        Action = "http://tempuri.org/IGameClientCallback/ReceiveMatchStarting",
        IsOneWay = true)]
    void ReceiveMatchStarting(MatchStartingMessage message);

    /// <summary>
    /// Receives an invitation from a friend to join a lobby.
    /// </summary>
    /// <param name="message">The username of the friend and the code of the lobby.</param>
    [OperationContract(
        Action = "http://tempuri.org/IGameClientCallback/ReceiveLobbyInvitation",
        IsOneWay = true)]
    void ReceiveLobbyInvitation(LobbyInvitationMessage message);

    /// <summary>
    /// Receives the public state of the board after a change.
    /// </summary>
    /// <param name="message">The turn, the boneyard size, the engine, the trains and the players.</param>
    [OperationContract(
        Action = "http://tempuri.org/IGameClientCallback/ReceiveMatchUpdate",
        IsOneWay = true)]
    void ReceiveMatchUpdate(MatchSnapshotMessage message);

    /// <summary>
    /// Receives the tiles of the hand of the player.
    /// </summary>
    /// <param name="message">The tiles of the hand.</param>
    [OperationContract(
        Action = "http://tempuri.org/IGameClientCallback/ReceiveHand",
        IsOneWay = true)]
    void ReceiveHand(HandMessage message);

    /// <summary>
    /// Receives the end of a round and the points of each player.
    /// </summary>
    /// <param name="message">The engine of the round, the reason it ended and the scores.</param>
    [OperationContract(
        Action = "http://tempuri.org/IGameClientCallback/ReceiveRoundEnded",
        IsOneWay = true)]
    void ReceiveRoundEnded(RoundEndedMessage message);

    /// <summary>
    /// Receives the end of the match.
    /// </summary>
    /// <param name="message">The reason the match ended.</param>
    [OperationContract(
        Action = "http://tempuri.org/IGameClientCallback/ReceiveMatchEnded",
        IsOneWay = true)]
    void ReceiveMatchEnded(MatchEndedMessage message);
}
