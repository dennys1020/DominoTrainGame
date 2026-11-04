namespace DominoTrainGame.Client.Models.Contracts;

public sealed class LobbySettings
{
    private const int EmptyMaximumPlayers = 0;

    static LobbySettings()
    {
        Empty = new LobbySettings(string.Empty, EmptyMaximumPlayers);
    }

    public LobbySettings(string code, int maximumPlayers)
    {
        Code = code ?? string.Empty;
        MaximumPlayers = maximumPlayers;
    }

    public static LobbySettings Empty { get; }

    public string Code { get; }

    public int MaximumPlayers { get; }
}
