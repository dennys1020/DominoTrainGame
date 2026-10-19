using System;
using System.Security.Cryptography;
using log4net.Ext.Trace;

namespace DominoTrainGame.Server.Application.Security;

public static class PasswordHasher
{
    private const byte SupportedFormatMarker = 0;
    private const int FormatMarkerIndex = 0;
    private const int SaltSize = 16;
    private const int SubkeySize = 32;
    private const int IterationCount = 1000;
    private const int SaltOffset = 1;
    private const int SubkeyOffset = SaltOffset + SaltSize;
    private const int HashSize = SubkeyOffset + SubkeySize;
    private const int DestinationOffset = 0;
    private const int NoDifference = 0;
    private static readonly ITraceLog _logger;

    static PasswordHasher()
    {
        _logger = TraceLogManager.GetLogger(typeof(PasswordHasher));
    }

    public static bool Verify(string hashedPassword, string password)
    {
        bool isMatch = false;

        if (!string.IsNullOrEmpty(hashedPassword) && password != null)
        {
            isMatch = VerifyEncodedHash(hashedPassword, password);
        }

        return isMatch;
    }

    private static bool VerifyEncodedHash(string hashedPassword, string password)
    {
        bool isMatch = false;

        try
        {
            byte[] decodedHash = Convert.FromBase64String(hashedPassword);
            bool isSupportedFormat = decodedHash.Length == HashSize
                && decodedHash[FormatMarkerIndex] == SupportedFormatMarker;

            if (isSupportedFormat)
            {
                isMatch = CompareSubkeys(decodedHash, password);
            }
        }
        catch (FormatException exception)
        {
            _logger.Warn("A stored password hash is not a valid Base64 value.", exception);
        }

        return isMatch;
    }

    private static bool CompareSubkeys(byte[] decodedHash, string password)
    {
        byte[] salt = new byte[SaltSize];
        byte[] storedSubkey = new byte[SubkeySize];
        byte[] computedSubkey;

        Buffer.BlockCopy(decodedHash, SaltOffset, salt, DestinationOffset, SaltSize);
        Buffer.BlockCopy(decodedHash, SubkeyOffset, storedSubkey, DestinationOffset, SubkeySize);

        using (Rfc2898DeriveBytes keyDerivation = new Rfc2898DeriveBytes(password, salt, IterationCount))
        {
            computedSubkey = keyDerivation.GetBytes(SubkeySize);
        }

        bool isMatch = AreEqualInConstantTime(storedSubkey, computedSubkey);
        return isMatch;
    }

    private static bool AreEqualInConstantTime(byte[] firstValue, byte[] secondValue)
    {
        int difference = firstValue.Length ^ secondValue.Length;

        for (int index = 0; index < firstValue.Length && index < secondValue.Length; index++)
        {
            difference |= firstValue[index] ^ secondValue[index];
        }

        return difference == NoDifference;
    }
}
