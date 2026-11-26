using System.Collections.Generic;
using System.ServiceModel;
using DominoTrainGame.Server.Service.DataContracts;
using DominoTrainGame.Server.Service.Faults;

namespace DominoTrainGame.Server.Service.Contracts;

/// <summary>
/// Receives the moves of the players during a match and reads the leaderboard.
/// </summary>
/// <remarks>
/// Every request carries the session token, and each valid call extends its expiration. The new state of
/// the match arrives through <see cref="IGameClientCallback"/> on the lobby channel.
/// </remarks>
[ServiceContract]
public interface IMatchService
{
    /// <summary>
    /// Plays a tile from the hand of the player on a train.
    /// </summary>
    /// <param name="request">The session token, the tile and the owner of the target train.</param>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="request"/> is null.
    /// </exception>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.InvalidInput"/> when the tile is missing,
    /// <see cref="GameServiceFaultCode.MatchNotFound"/> when the player is not in a match,
    /// <see cref="GameServiceFaultCode.NotYourTurn"/> when it is not the turn of the player,
    /// <see cref="GameServiceFaultCode.TileNotInHand"/> when the player does not have the tile,
    /// <see cref="GameServiceFaultCode.TileDoesNotMatch"/> when the tile does not match the end of the train,
    /// <see cref="GameServiceFaultCode.TrainNotAvailable"/> when the train is closed to the player,
    /// <see cref="GameServiceFaultCode.MustSatisfyDouble"/> when an open double must be covered first, or
    /// <see cref="GameServiceFaultCode.SessionNotFound"/> when the session token is unknown or expired.
    /// </exception>
    [OperationContract]
    [FaultContract(typeof(GameServiceFault))]
    void PlayTile(PlayTileRequest request);

    /// <summary>
    /// Takes a tile from the boneyard.
    /// </summary>
    /// <param name="request">The session token.</param>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.MatchNotFound"/> when the player is not in a match,
    /// <see cref="GameServiceFaultCode.NotYourTurn"/> when it is not the turn of the player,
    /// <see cref="GameServiceFaultCode.BoneyardEmpty"/> when the boneyard has no tiles,
    /// <see cref="GameServiceFaultCode.AlreadyDrawn"/> when the player already drew a tile this turn, or
    /// <see cref="GameServiceFaultCode.SessionNotFound"/> when the session token is unknown or expired.
    /// </exception>
    [OperationContract]
    [FaultContract(typeof(GameServiceFault))]
    void DrawTile(SessionTokenRequest request);

    /// <summary>
    /// Ends the turn of a player that cannot play.
    /// </summary>
    /// <param name="request">The session token.</param>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.MatchNotFound"/> when the player is not in a match,
    /// <see cref="GameServiceFaultCode.NotYourTurn"/> when it is not the turn of the player,
    /// <see cref="GameServiceFaultCode.MustDrawFirst"/> when the player must draw a tile before passing, or
    /// <see cref="GameServiceFaultCode.SessionNotFound"/> when the session token is unknown or expired.
    /// </exception>
    [OperationContract]
    [FaultContract(typeof(GameServiceFault))]
    void PassTurn(SessionTokenRequest request);

    /// <summary>
    /// Removes the player from the match. The match continues while at least two players stay.
    /// </summary>
    /// <param name="leaveMatchRequest">The session token.</param>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.MatchNotFound"/> when the player is not in a match, or
    /// <see cref="GameServiceFaultCode.SessionNotFound"/> when the session token is unknown or expired.
    /// </exception>
    [OperationContract]
    [FaultContract(typeof(GameServiceFault))]
    void LeaveMatch(SessionTokenRequest leaveMatchRequest);

    /// <summary>
    /// Gets the registered players with the most wins.
    /// </summary>
    /// <param name="leaderboardRequest">The session token.</param>
    /// <returns>A limited list of entries ordered by rank, or an empty list when no match was saved.</returns>
    /// <exception cref="FaultException{GameServiceFault}">
    /// Thrown with <see cref="GameServiceFaultCode.SessionNotFound"/> when the session token is unknown or expired, or
    /// <see cref="GameServiceFaultCode.DatabaseUnavailable"/> when the database cannot be reached.
    /// </exception>
    [OperationContract]
    [FaultContract(typeof(GameServiceFault))]
    List<LeaderboardEntryData> GetLeaderboard(SessionTokenRequest leaderboardRequest);
}
