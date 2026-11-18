using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.UseCases;

public interface ISignUpUseCases
{
    bool SignUp(string username, string email, string password);

    VerifyEmailStatus VerifyEmail(string email, string verificationCode);

    bool ResendVerificationCode(string email);
}
