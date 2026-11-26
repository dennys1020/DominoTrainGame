namespace DominoTrainGame.Shared.Common;

public static class CollectionIndex
{
    public const int First = 0;
    public const int NotFound = -1;

    public static bool IsFound(int index)
    {
        bool isFound = index > NotFound;
        return isFound;
    }
}
