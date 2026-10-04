using System;
using System.Globalization;
using System.Security.Cryptography;

namespace DominoTrainGame.Utils;

public sealed class VerificationCodeService
{
    public const int ValidityMinutes = 10;

    private const int MaximumFailedAttempts = 3;
    private const int CodeUpperBound = 1000000;
    private const string CodeFormat = "D6";

    private string _code = string.Empty;
    private DateTime _expiresAtUtc = DateTime.MinValue;
    private int _failedAttempts;

    public string GenerateCode()
    {
        byte[] randomBytes = new byte[4];

        using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
        {
            generator.GetBytes(randomBytes);
        }

        uint randomValue = BitConverter.ToUInt32(randomBytes, 0);

        _code = (randomValue % CodeUpperBound).ToString(CodeFormat, CultureInfo.InvariantCulture);
        _expiresAtUtc = DateTime.UtcNow.AddMinutes(ValidityMinutes);
        _failedAttempts = 0;

        return _code;
    }

    public VerificationCodeStatus Verify(string enteredCode)
    {
        VerificationCodeStatus status;

        if (_failedAttempts >= MaximumFailedAttempts)
        {
            status = VerificationCodeStatus.TooManyAttempts;
        }
        else if (DateTime.UtcNow > _expiresAtUtc)
        {
            status = VerificationCodeStatus.Expired;
        }
        else if (AreEqual(_code, enteredCode))
        {
            status = VerificationCodeStatus.Valid;
        }
        else
        {
            _failedAttempts++;

            status = _failedAttempts >= MaximumFailedAttempts
                ? VerificationCodeStatus.TooManyAttempts
                : VerificationCodeStatus.Invalid;
        }

        return status;
    }

    private static bool AreEqual(string expected, string actual)
    {
        int difference = expected.Length ^ actual.Length;

        for (int index = 0; index < expected.Length && index < actual.Length; index++)
        {
            difference |= expected[index] ^ actual[index];
        }

        return difference == 0;
    }
}
