using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DominoTrainGame.Server.Application.Ports;

namespace DominoTrainGame.Server.Application.Security;

public sealed class LobbyCodeGenerator : ILobbyCodeGenerator
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int CodeLength = 6;
    private const int InvalidCharacterIndex = -1;

    public string Generate()
    {
        byte[] randomBytes = new byte[CodeLength];

        using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
        {
            generator.GetBytes(randomBytes);
        }

        StringBuilder code = new StringBuilder(CodeLength);

        foreach (byte randomByte in randomBytes)
        {
            code.Append(Alphabet[randomByte % Alphabet.Length]);
        }

        string lobbyCode = code.ToString();
        return lobbyCode;
    }

    public bool IsValid(string code)
    {
        bool isValidCode = code.Length == CodeLength && code.All(IsAllowedCharacter);
        return isValidCode;
    }

    private static bool IsAllowedCharacter(char codeCharacter)
    {
        bool isAllowed = Alphabet.IndexOf(codeCharacter) > InvalidCharacterIndex;
        return isAllowed;
    }
}
