using System.Threading.Tasks;

namespace DominoTrainGame.Server.Application.Ports;

public interface IEmailSender
{
    Task<bool> SendVerificationCodeAsync(string recipientEmail, string code);
}
