using System;
using System.Security.Cryptography;
using DominoTrainGame.Shared.Common;

namespace DominoTrainGame.Server.Application.Security;

internal static class SecureRandomNumber
{

    public static uint Next(uint exclusiveUpperBound)
    {
        byte[] randomBytes = new byte[sizeof(uint)];

        using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
        {
            generator.GetBytes(randomBytes);
        }

        uint randomValue = BitConverter.ToUInt32(randomBytes, CollectionIndex.First);
        uint boundedValue = randomValue % exclusiveUpperBound;
        return boundedValue;
    }
}
