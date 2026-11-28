using System;
using System.Globalization;
using System.Net.Mail;
using System.Resources;
using DominoTrainGame.Server.Application.Errors;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Shared.Common;

namespace DominoTrainGame.Server.Persistence.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private const string TemplatesBaseName = "DominoTrainGame.Server.Persistence.Email.EmailTemplates";
    private const string VerificationSubjectKey = "VerificationEmailSubject";
    private const string VerificationBodyFormatKey = "VerificationEmailBodyFormat";

    private static readonly ResourceManager _emailTemplates;
    private static readonly CultureInfo _englishCulture;

    static SmtpEmailSender()
    {
        _emailTemplates = new ResourceManager(TemplatesBaseName, typeof(SmtpEmailSender).Assembly);
        _englishCulture = CultureInfo.GetCultureInfo(GameLanguage.EnglishCode);
    }

    public void SendVerificationCode(string recipientEmail, string verificationCode, string language)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
        {
            throw new ArgumentException("The recipient email cannot be empty.", nameof(recipientEmail));
        }

        if (string.IsNullOrWhiteSpace(verificationCode))
        {
            throw new ArgumentException("The verification code cannot be empty.", nameof(verificationCode));
        }

        CultureInfo emailCulture = ResolveCulture(language);

        using (MailMessage message = CreateVerificationMessage(recipientEmail, verificationCode, emailCulture))
        {
            Send(message);
        }
    }

    private static CultureInfo ResolveCulture(string language)
    {
        bool isEnglish = GameLanguage.IsEnglish(language);
        CultureInfo emailCulture = isEnglish ? _englishCulture : CultureInfo.InvariantCulture;
        return emailCulture;
    }

    private static MailMessage CreateVerificationMessage(
        string recipientEmail,
        string verificationCode,
        CultureInfo emailCulture)
    {
        MailMessage message = new MailMessage();
        message.To.Add(recipientEmail);
        message.Subject = GetTemplate(VerificationSubjectKey, emailCulture);
        message.Body = string.Format(
            CultureInfo.InvariantCulture,
            GetTemplate(VerificationBodyFormatKey, emailCulture),
            verificationCode);
        return message;
    }

    private static string GetTemplate(string templateKey, CultureInfo emailCulture)
    {
        string template = _emailTemplates.GetString(templateKey, emailCulture);

        if (string.IsNullOrEmpty(template))
        {
            throw new InvalidOperationException("The email template is missing from the resources.");
        }

        return template;
    }

    private static void Send(MailMessage message)
    {
        using (SmtpClient smtpClient = new SmtpClient { EnableSsl = true })
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
