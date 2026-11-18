using System;
using System.Security.Cryptography;

namespace DominoTrainGame.Server.Application.Security;

internal static class SecureRandomNumber
{
    private const int RandomValueStartIndex = 0;

    public static uint Next(uint exclusiveUpperBound)
    {
        byte[] randomBytes = new byte[sizeof(uint)];

        using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
        {
            generator.GetBytes(randomBytes);
        }

        uint randomValue = BitConverter.ToUInt32(randomBytes, RandomValueStartIndex);
        uint boundedValue = randomValue % exclusiveUpperBound;
        return boundedValue;
    }
}
