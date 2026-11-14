using System;
using System.Globalization;
using System.Security.Cryptography;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Application.Security;

public sealed class VerificationCodeGenerator : IVerificationCodeGenerator
{
    private const uint CodeRange = 1000000;
    private const string CodeFormat = "D6";

    public string Generate()
    {
        byte[] randomBytes = new byte[sizeof(uint)];

        using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
        {
            generator.GetBytes(randomBytes);
        }

        uint randomValue = BitConverter.ToUInt32(randomBytes, 0);
        uint codeValue = randomValue % CodeRange;
        string verificationCode = codeValue.ToString(CodeFormat, CultureInfo.InvariantCulture);
        return verificationCode;
    }
}
