namespace DominoTrainGame.Server.Application.Ports;

/// <summary>
/// Creates the codes sent by email to verify a new account.
/// </summary>
public interface IVerificationCodeGenerator
{
    /// <summary>
    /// Creates a random verification code.
    /// </summary>
    /// <returns>A code of six digits that keeps the leading zeros, such as <c>004219</c>.</returns>
    string Generate();
}
