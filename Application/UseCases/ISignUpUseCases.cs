using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.UseCases;

/// <summary>
/// Registers new accounts after their email is verified.
/// </summary>
/// <remarks>
/// The account is created only after the player enters the code sent by email. Until then, the registration
/// waits in server memory.
/// </remarks>
public interface ISignUpUseCases
{
    /// <summary>
    /// Validates a new account and sends a verification code to its email.
    /// </summary>
    /// <param name="details">The username, email and password of the new account.</param>
    /// <param name="language">The language of the client, which selects the language of the email.</param>
    /// <returns><c>true</c> when the code was sent; <c>false</c> when the username or the email is in use.</returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="details"/> is null.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    /// Thrown when the username, the email or the password is empty, too long, too short or malformed.
    /// </exception>
    bool SignUp(SignUpDetails details, string language);

    /// <summary>
    /// Checks the code of a pending registration and creates the account when the code is correct.
    /// </summary>
    /// <param name="email">The email of the pending registration.</param>
    /// <param name="verificationCode">The code entered by the player.</param>
    /// <returns>
    /// <see cref="VerifyEmailStatus.Success"/> when the account was created, or the reason it was rejected.
    /// </returns>
    VerifyEmailStatus VerifyEmail(string email, string verificationCode);

    /// <summary>
    /// Sends a new verification code to the email of a pending registration.
    /// </summary>
    /// <param name="email">The email of the pending registration.</param>
    /// <param name="language">The language of the client, which selects the language of the email.</param>
    /// <returns>The result of the resend, or the reason it was rejected.</returns>
    ResendVerificationCodeStatus ResendVerificationCode(string email, string language);
}
