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
        bool isValidCode = true;
        string normalizedCode = code ?? string.Empty;
        bool hasValidLength = normalizedCode.Length == CodeLength;

        if (!hasValidLength)
        {
            isValidCode = false;
        }

        foreach (char codeCharacter in normalizedCode)
        {
            bool isCharacterAllowed = Alphabet.IndexOf(codeCharacter) > InvalidCharacterIndex;

            if (!isCharacterAllowed)
            {
                isValidCode = false;
            }
        }

        return isValidCode;
    }
}
