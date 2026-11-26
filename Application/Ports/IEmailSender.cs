namespace DominoTrainGame.Server.Application.Ports;

/// <summary>
/// Sends the emails of the account flows.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends the email with the code that verifies a new account.
    /// </summary>
    /// <param name="recipientEmail">The email address of the new account.</param>
    /// <param name="verificationCode">The code that the player must enter.</param>
    /// <param name="language">
    /// The language of the client, such as <c>en-US</c>. A language that starts with <c>en</c> uses the
    /// English template, and any other value uses the default Spanish template.
    /// </param>
    /// <exception cref="System.ArgumentException">
    /// Thrown when <paramref name="recipientEmail"/> or <paramref name="verificationCode"/> is empty.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown when the email template is missing from the resources.
    /// </exception>
    /// <exception cref="Errors.EmailUnavailableException">
    /// Thrown when the mail server cannot send the email.
    /// </exception>
    void SendVerificationCode(string recipientEmail, string verificationCode, string language);
}
