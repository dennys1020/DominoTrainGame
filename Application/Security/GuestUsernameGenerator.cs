using System.Globalization;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Application.Security;

public sealed class GuestUsernameGenerator : IGuestUsernameGenerator
{
    private const string GuestUsernamePrefix = "Guest";
    private const uint MinimumGuestNumber = 100000;
    private const uint GuestNumberRange = 900000;

    public string Generate()
    {
        uint guestNumber = MinimumGuestNumber + SecureRandomNumber.Next(GuestNumberRange);
        string guestUsername = GuestUsernamePrefix + guestNumber.ToString(CultureInfo.InvariantCulture);
        return guestUsername;
    }
}
