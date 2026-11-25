using System;
using System.Linq;
using System.Transactions;
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

        using (TransactionScope transaction = SerializableTransaction.Begin())
        {
            bool isUsernameTaken = IsUsernameTaken(context);

            if (!isUsernameTaken)
            {
                guest = InsertGuest(context);
            }

            transaction.Complete();
        }

        return guest;
    }

    private bool IsUsernameTaken(DominoGameDBEntities context)
    {
        bool isTaken = context.Players.Any(player => player.Username == _username);
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

        PlayerIdentity guest = PlayerIdentity.CreateGuest(player.PlayerId, player.Username);
        return guest;
    }
}
