using System.Globalization;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Application.Security;

public sealed class VerificationCodeGenerator : IVerificationCodeGenerator
{
    private const uint CodeRange = 1000000;
    private const string CodeFormat = "D6";

    public string Generate()
    {
        uint codeValue = SecureRandomNumber.Next(CodeRange);
        string verificationCode = codeValue.ToString(CodeFormat, CultureInfo.InvariantCulture);
        return verificationCode;
    }
}
