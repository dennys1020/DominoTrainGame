namespace DominoTrainGame.Server.Persistence.Data.Repositories;

internal static class ContextChanges
{
    private const int NoSavedRows = 0;

    public static bool Save(DominoGameDBEntities context)
    {
        bool wasSaved = context.SaveChanges() > NoSavedRows;
        return wasSaved;
    }
}
