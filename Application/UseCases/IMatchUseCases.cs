using System.Collections.Generic;
using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.UseCases;

/// <summary>
/// Applies the player moves of the matches that are being played.
/// </summary>
/// <remarks>
/// The server keeps the only valid state of each match. After every accepted move the players receive the
/// new board, and each player receives only the tiles of their own hand.
/// </remarks>
public interface IMatchUseCases
{
    /// <summary>
    /// Shuffles the tiles, deals the hands and starts the first round of a match.
    /// </summary>
    /// <param name="code">The code of the lobby, which becomes the code of the match.</param>
    /// <param name="playerIds">The players of the match, in turn order.</param>
    /// <param name="dominoSet">The domino set selected in the lobby.</param>
    /// <returns>
    /// <c>true</c> when the match started; <c>false</c> when the code or one of the players is already in a
    /// match.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="code"/>, <paramref name="playerIds"/> or <paramref name="dominoSet"/> is
    /// null.
    /// </exception>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// Thrown when <paramref name="dominoSet"/> does not support the number of players.
    /// </exception>
    bool StartMatch(string code, IReadOnlyList<int> playerIds, DominoSet dominoSet);

    /// <summary>
    /// Plays a tile from the hand of a player on the train of the selected owner.
    /// </summary>
    /// <remarks>
    /// The round ends when the player plays the last tile, and the match ends after the last round.
    /// </remarks>
    /// <param name="playerId">The player that plays the tile.</param>
    /// <param name="tile">The tile to play.</param>
    /// <param name="trainOwnerId">The owner of the target train.</param>
    /// <returns>The result of the move, or the reason it was rejected.</returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="tile"/> is null.
    /// </exception>
    PlayTileStatus PlayTile(int playerId, Tile tile, int trainOwnerId);

    /// <summary>
    /// Moves a tile from the boneyard to the hand of the player whose turn it is.
    /// </summary>
    /// <param name="playerId">The player that draws.</param>
    /// <returns>The result of the draw, or the reason it was rejected.</returns>
    DrawTileStatus DrawTile(int playerId);

    /// <summary>
    /// Ends the turn of a player that cannot play, which opens the train of that player.
    /// </summary>
    /// <param name="playerId">The player that passes.</param>
    /// <returns>The result of the pass, or the reason it was rejected.</returns>
    PassTurnStatus PassTurn(int playerId);

    /// <summary>
    /// Sends the current board and hand to a player, for example when the match window opens or after a
    /// reconnection.
    /// </summary>
    /// <param name="playerId">The player that receives the state.</param>
    /// <returns><c>true</c> when the state was sent; <c>false</c> when the player is not in a match.</returns>
    bool SendMatchState(int playerId);

    /// <summary>
    /// Removes a player from the match and returns the hand of that player to the boneyard.
    /// </summary>
    /// <remarks>
    /// The match continues for the others, and it ends when fewer than two players stay.
    /// </remarks>
    /// <param name="playerId">The player that leaves.</param>
    /// <returns><c>true</c> when the player left; <c>false</c> when the player is not in a match.</returns>
    bool LeaveMatch(int playerId);
}
