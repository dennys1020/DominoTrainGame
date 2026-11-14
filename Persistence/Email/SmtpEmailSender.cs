using System;
using System.Globalization;
using System.Net.Mail;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Persistence.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private const string VerificationSubject = "Domino Train - Código de verificación / Verification code";
    private const string VerificationBodyFormat =
        "Tu código de verificación es: {0}\r\n\r\nYour verification code is: {0}";

    public void SendVerificationCode(string recipientEmail, string verificationCode)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
        {
            throw new ArgumentException("The recipient email cannot be empty.", nameof(recipientEmail));
        }

        if (string.IsNullOrWhiteSpace(verificationCode))
        {
            throw new ArgumentException("The verification code cannot be empty.", nameof(verificationCode));
        }

        using (MailMessage message = CreateVerificationMessage(recipientEmail, verificationCode))
        {
            Send(message);
        }
    }

    private static MailMessage CreateVerificationMessage(string recipientEmail, string verificationCode)
    {
        MailMessage message = new MailMessage();
        message.To.Add(recipientEmail);
        message.Subject = VerificationSubject;
        message.Body = string.Format(CultureInfo.InvariantCulture, VerificationBodyFormat, verificationCode);
        return message;
    }

    private static void Send(MailMessage message)
    {
        using (SmtpClient smtpClient = new SmtpClient())
        {
            try
            {
                smtpClient.Send(message);
            }
            catch (SmtpException exception)
            {
                throw new EmailUnavailableException("The verification email could not be sent.", exception);
            }
        }
    }
}
