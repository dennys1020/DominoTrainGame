using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using DominoTrainGame.Server.Application.Domain;

namespace DominoTrainGame.Server.Persistence.Data.Repositories;

internal sealed class GuestPlayerCreation
{
    private const byte GuestPlayerFlag = 1;

    private readonly string _username;

    public GuestPlayerCreation(string username)
    {
        _username = username ?? throw new ArgumentNullException(nameof(username));
    }

    public PlayerIdentity Create(DominoGameDBEntities context)
    {
        PlayerIdentity guest = PlayerIdentity.Empty;

        using (DbContextTransaction transaction = context.Database.BeginTransaction(IsolationLevel.Serializable))
        {
            bool isUsernameTaken = IsUsernameTaken(context);

            if (!isUsernameTaken)
            {
                guest = InsertGuest(context);
            }

            transaction.Commit();
        }

        return guest;
    }

    private bool IsUsernameTaken(DominoGameDBEntities context)
    {
        IQueryable<int> matchingPlayerIds =
            from player in context.Players
            where player.Username == _username
            select player.PlayerId;
        bool isTaken = matchingPlayerIds.Any();
        return isTaken;
    }

    private PlayerIdentity InsertGuest(DominoGameDBEntities context)
    {
        Player player = new Player
        {
            Username = _username,
            IsGuest = GuestPlayerFlag
        };

        context.Players.Add(player);
        context.SaveChanges();

        PlayerIdentity guest = new PlayerIdentity(player.PlayerId, player.Username);
        return guest;
    }
}
