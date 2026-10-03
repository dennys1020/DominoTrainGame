using Microsoft.AspNet.Identity;
using AspNetPasswordHasher = Microsoft.AspNet.Identity.PasswordHasher;

namespace DominoTrainGame;


public static class PasswordHasher
{
    private static readonly AspNetPasswordHasher _passwordHasher;

    static PasswordHasher()
    {
        _passwordHasher = new AspNetPasswordHasher();
    }

    public static string Hash(string password)
    {
        string hashedPassword = _passwordHasher.HashPassword(password);

        return hashedPassword;
    }

    public static bool Verify(string hashedPassword, string password)
    {
        PasswordVerificationResult verificationResult =
            _passwordHasher.VerifyHashedPassword(hashedPassword, password);

        bool isMatch = verificationResult == PasswordVerificationResult.Success;

        return isMatch;
    }
}