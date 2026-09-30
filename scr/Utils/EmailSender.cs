#nullable enable

using System;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using DominoTrainGame.Resources.Localization;
using log4net.Ext.Trace;

namespace DominoTrainGame.Utils;
public sealed class EmailSender
{
    private const string HostKey = "SmtpHost";
    private const string PortKey = "SmtpPort";
    private const string UserKey = "SmtpUser";
    private const string PasswordKey = "SmtpPassword";

    private static readonly ITraceLog _logger;

    static EmailSender()
    {
        _logger = TraceLogManager.GetLogger(typeof(EmailSender));
    }

    public async Task<bool> SendVerificationCodeAsync(string recipientEmail, string code)
    {
        bool wasSent = false;

        try
        {
            string host = ReadSetting(HostKey);
            int port = int.Parse(ReadSetting(PortKey), CultureInfo.InvariantCulture);
            string user = ReadSetting(UserKey);
            string password = ReadSetting(PasswordKey);

            string subject = UiStrings.VerificationCodeEmailSubject;
            string body = string.Format(
                CultureInfo.CurrentCulture,
                UiStrings.VerificationCodeEmailBody,
                code,
                VerificationCodeService.ValidityMinutes);

            using (SmtpClient smtpClient = new SmtpClient(host, port))
            using (MailMessage message = new MailMessage(user, recipientEmail, subject, body))
            {
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential(user, password);

                await smtpClient.SendMailAsync(message);
            }

            wasSent = true;
        }

        catch (SmtpException exception)
        {
            System.Diagnostics.Debug.WriteLine(exception.ToString());
            _logger.Error("The verification email could not be sent by the SMTP server.", exception);
        }
        catch (FormatException exception)
        {
            System.Diagnostics.Debug.WriteLine(exception.ToString());
            _logger.Error("The SMTP port or an email address has an invalid format.", exception);
        }
        catch (InvalidOperationException exception)
        {
            System.Diagnostics.Debug.WriteLine(exception.ToString());
            _logger.Error("The SMTP settings are missing from the configuration.", exception);
        }

        return wasSent;
    }

    private static string ReadSetting(string key)
    {
        string? value = ConfigurationManager.AppSettings[key];

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("The setting '" + key + "' is not configured.");
        }

        return value!;
    }
}