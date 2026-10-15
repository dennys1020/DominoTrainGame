using DominoTrainGame.Server.Application.Ports;
using System;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Resources;
using System.Threading.Tasks;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Verification;
#nullable enable

namespace DominoTrainGame.Server.Application.Notifications;

public sealed class EmailSender : IEmailSender
{
    private const string HostKey = "SmtpHost";
    private const string PortKey = "SmtpPort";
    private const string UserKey = "SmtpUser";
    private const string PasswordKey = "SmtpPassword";
    private const int MinimumPort = 1;
    private const int MaximumPort = 65535;

    private static readonly ITraceLog _logger;
    private static readonly ResourceManager _templates = new ResourceManager(
        "DominoTrainGame.Server.Application.Notifications.EmailTemplates", typeof(EmailSender).Assembly);

    static EmailSender()
    {
        _logger = TraceLogManager.GetLogger(typeof(EmailSender));
    }

    public async Task<bool> SendVerificationCodeAsync(string recipientEmail, string code, string language)
    {
        bool wasSent = false;

        try
        {
            string host = ReadSetting(HostKey);
            bool hasPort = int.TryParse(ReadSetting(PortKey), NumberStyles.None,
                CultureInfo.InvariantCulture, out int port);
            if (!hasPort || port < MinimumPort || port > MaximumPort)
            {
                throw new InvalidOperationException("The SMTP port is not configured correctly.");
            }
            string user = ReadSetting(UserKey);
            string password = ReadSetting(PasswordKey);

            CultureInfo culture = CultureInfo.GetCultureInfo(language == "es" ? "es" : "en");
            string subject = _templates.GetString("VerificationCodeEmailSubject", culture)!;
            string body = string.Format(culture,
                _templates.GetString("VerificationCodeEmailBody", culture)!,
                code, VerificationCodeService.ValidityMinutes);

            using (SmtpClient smtpClient = new SmtpClient(host, port))
            using (MailMessage message = new MailMessage(user, recipientEmail, subject, body))
            {
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential(user, password);

                await smtpClient.SendMailAsync(message).ConfigureAwait(false);
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
        catch (ConfigurationErrorsException exception)
        {
            _logger.Error("The SMTP configuration could not be read.", exception);
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
