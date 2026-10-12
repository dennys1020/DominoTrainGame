using DominoTrainGame.Utils;

namespace DominoTrainGame.Contracts;

public interface IVerificationSession
{
    string GenerateCode();
    VerificationCodeStatus Verify(string enteredCode);
}
