using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.UseCases;

public interface ISignUpUseCases
{
    bool SignUp(SignUpDetails detailsSignUp, string language);

    VerifyEmailStatus VerifyEmail(string email, string verificationCode);

    ResendVerificationCodeStatus ResendVerificationCode(string email, string language);
}
