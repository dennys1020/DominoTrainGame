namespace DominoTrainGame.Client.ViewModel.Validation;

public static class InputText
{
    public const int EmptyLength = 0;

    public static string Normalize(string text)
    {
        string normalizedText = text?.Trim() ?? string.Empty;
        return normalizedText;
    }
}
