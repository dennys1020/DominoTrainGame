using System;
using System.Globalization;
using System.Security.Cryptography;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Application.Security;

public sealed class GuestUsernameGenerator : IGuestUsernameGenerator
{
    private const string GuestUsernamePrefix = "Invitado";
    private const uint MinimumGuestNumber = 100000;
    private const uint GuestNumberRange = 900000;
    private const int RandomValueStartIndex = 0;

    public string Generate()
    {
        byte[] randomBytes = new byte[sizeof(uint)];

        using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
        {
            generator.GetBytes(randomBytes);
        }

        uint randomValue = BitConverter.ToUInt32(randomBytes, RandomValueStartIndex);
        uint guestNumber = MinimumGuestNumber + randomValue % GuestNumberRange;
        string guestUsername = GuestUsernamePrefix + guestNumber.ToString(CultureInfo.InvariantCulture);
        return guestUsername;
    }
}
