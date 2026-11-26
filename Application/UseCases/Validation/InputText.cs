namespace DominoTrainGame.Server.Application.UseCases.Validation;

public static class InputText
{
    public static string Normalize(string text)
    {
        string normalizedText = text?.Trim() ?? string.Empty;
        return normalizedText;
    }
}
