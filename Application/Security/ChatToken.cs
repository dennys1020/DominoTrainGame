namespace DominoTrainGame.Server.Application.Security;

internal sealed class ChatToken
{
    public ChatToken(int start, string text)
    {
        Start = start;
        Text = text ?? string.Empty;
    }

    public int Start { get; }

    public string Text { get; }
}
