namespace DominoTrainGame.Server.Application.Ports;

public interface IEmailSender
{
    void SendVerificationCode(string recipientEmail, string verificationCode);
}
