using DominoTrainGame.Server.Application.Domain;
using DominoTrainGame.Server.Application.Results;

namespace DominoTrainGame.Server.Application.UseCases;

public interface IInvitationUseCases
{
    InviteFriendStatus InviteFriend(LobbyMember inviter, string friendUsername);
}
