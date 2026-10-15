using System;
using System.Globalization;
using System.Security.Cryptography;
using DominoTrainGame.Server.Application.Results;
using DominoTrainGame.Server.Application.Security;

namespace DominoTrainGame.Server.Application.Verification;

public sealed class VerificationCodeService
{
    public const int ValidityMinutes = 10;
    private const int MaximumFailedAttempts = 3;
    private const uint CodeUpperBound = 1000000;
    private const string CodeFormat = "D6";
    private readonly Func<DateTime> _utcNow;
    private string _codeHash = string.Empty;
    private DateTime _expiresAtUtc = DateTime.MinValue;
    private int _failedAttempts;

    public VerificationCodeService() : this(GetUtcNow) { }

    public VerificationCodeService(Func<DateTime> utcNow)
    {
        _utcNow = utcNow ?? throw new ArgumentNullException(nameof(utcNow));
    }

    public string GenerateCode()
    {
        byte[] randomBytes = new byte[sizeof(uint)];
        uint randomValue;
        uint exclusiveLimit = uint.MaxValue - uint.MaxValue % CodeUpperBound;

        using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
        {
            do
            {
                generator.GetBytes(randomBytes);
                randomValue = BitConverter.ToUInt32(randomBytes, 0);
            }
            while (randomValue >= exclusiveLimit);
        }

        string code = (randomValue % CodeUpperBound).ToString(CodeFormat, CultureInfo.InvariantCulture);
        _codeHash = PasswordHasher.Hash(code);
        _expiresAtUtc = _utcNow().AddMinutes(ValidityMinutes);
        _failedAttempts = 0;

        return code;
    }

    public VerificationCodeStatus Verify(string enteredCode)
    {
        VerificationCodeStatus status;

        if (_failedAttempts >= MaximumFailedAttempts)
        {
            status = VerificationCodeStatus.TooManyAttempts;
        }
        else if (_utcNow() >= _expiresAtUtc || string.IsNullOrEmpty(_codeHash))
        {
            status = VerificationCodeStatus.Expired;
        }
        else if (enteredCode != null && PasswordHasher.Verify(_codeHash, enteredCode))
        {
            status = VerificationCodeStatus.Valid;
        }
        else
        {
            _failedAttempts++;
            status = _failedAttempts >= MaximumFailedAttempts
                ? VerificationCodeStatus.TooManyAttempts : VerificationCodeStatus.Invalid;
        }

        return status;
    }

    private static DateTime GetUtcNow()
    {
        DateTime dateTime = DateTime.UtcNow;
        return dateTime;
    }
}
