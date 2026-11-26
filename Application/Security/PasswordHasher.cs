using System;
using System.Security.Cryptography;
using log4net.Ext.Trace;
using DominoTrainGame.Server.Application.Ports;
using DominoTrainGame.Shared.Common;

namespace DominoTrainGame.Server.Application.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    private const byte SupportedFormatMarker = 1;
    private const int SaltSize = 16;
    private const int SubkeySize = 32;
    private const int IterationCount = 600000;
    private const int SaltOffset = sizeof(byte);
    private const int SubkeyOffset = SaltOffset + SaltSize;
    private const int HashSize = SubkeyOffset + SubkeySize;
    private const int NoDifference = 0;

    private static readonly ITraceLog _logger;

    static PasswordHasher()
    {
        _logger = TraceLogManager.GetLogger(typeof(PasswordHasher));
    }

    public string Hash(string password)
    {
        if (password == null)
        {
            throw new ArgumentNullException(nameof(password));
        }

        byte[] salt = GenerateSalt();
        byte[] subkey = DeriveSubkey(password, salt);
        byte[] encodedHash = new byte[HashSize];

        encodedHash[CollectionIndex.First] = SupportedFormatMarker;
        Buffer.BlockCopy(salt, CollectionIndex.First, encodedHash, SaltOffset, SaltSize);
        Buffer.BlockCopy(subkey, CollectionIndex.First, encodedHash, SubkeyOffset, SubkeySize);

        string hashedPassword = Convert.ToBase64String(encodedHash);
        return hashedPassword;
    }

    public bool Verify(string hashedPassword, string password)
    {
        bool isMatch = false;
        string storedHash = hashedPassword ?? string.Empty;
        string enteredPassword = password ?? string.Empty;
        bool canVerify = storedHash.Length > InputText.EmptyLength && enteredPassword.Length > InputText.EmptyLength;

        if (canVerify)
        {
            isMatch = VerifyEncodedHash(storedHash, enteredPassword);
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
                && decodedHash[CollectionIndex.First] == SupportedFormatMarker;

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

        Buffer.BlockCopy(decodedHash, SaltOffset, salt, CollectionIndex.First, SaltSize);
        Buffer.BlockCopy(decodedHash, SubkeyOffset, storedSubkey, CollectionIndex.First, SubkeySize);

        byte[] computedSubkey = DeriveSubkey(password, salt);
        bool isMatch = AreEqualInConstantTime(storedSubkey, computedSubkey);
        return isMatch;
    }

    private static bool AreEqualInConstantTime(byte[] firstValue, byte[] secondValue)
    {
        int difference = firstValue.Length ^ secondValue.Length;

        for (int index = CollectionIndex.First; index < firstValue.Length && index < secondValue.Length; index++)
        {
            difference |= firstValue[index] ^ secondValue[index];
        }

        bool areEqual = difference == NoDifference;
        return areEqual;
    }

    private static byte[] GenerateSalt()
    {
        byte[] salt = new byte[SaltSize];

        using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
        {
            generator.GetBytes(salt);
        }

        return salt;
    }

    private static byte[] DeriveSubkey(string password, byte[] salt)
    {
        byte[] subkey;

        using (Rfc2898DeriveBytes keyDerivation = new Rfc2898DeriveBytes(
            password,
            salt,
            IterationCount,
            HashAlgorithmName.SHA256))
        {
            subkey = keyDerivation.GetBytes(SubkeySize);
        }

        return subkey;
    }
}
