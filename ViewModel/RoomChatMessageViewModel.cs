namespace DominoTrainGame.Client.ViewModel;

public sealed class RoomChatMessageViewModel
{
    public RoomChatMessageViewModel(string senderUsername, string text)
    {
        SenderUsername = senderUsername ?? string.Empty;
        Text = text ?? string.Empty;
    }

    public string SenderUsername { get; }

    public string Text { get; }
}
