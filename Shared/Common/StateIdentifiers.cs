namespace DominoTrainGame.Shared.Common;

public static class StateIdentifiers
{
    public const int EmptyPlayerId = 0;
    public const long EmptyVersion = 0;

    public static bool IsNewerVersion(long candidateVersion, long currentVersion)
    {
        bool isNewerVersion = candidateVersion > currentVersion;
        return isNewerVersion;
    }
}
