namespace DominoTrainGame.Server.Application.Ports;

/// <summary>
/// Creates the usernames of the guest players.
/// </summary>
public interface IGuestUsernameGenerator
{
    /// <summary>
    /// Creates a random guest username, such as <c>Guest482913</c>.
    /// </summary>
    /// <returns>A username that starts with <c>Guest</c> and ends with six random digits.</returns>
    string Generate();
}
