using System;
using System.ServiceModel;
using DominoTrainGame.Server.Application.Authentication;
using DominoTrainGame.Server.Persistence.Contracts;

namespace DominoTrainGame.Server.Service.Implementation;

[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
public sealed partial class GameService
{
    public GameService(IAuthenticationStore store, ILeaderboardSource leaderboardSource)
    {
        _loginValidator = new LoginValidator(store);
        _signUpValidator = new SignUpValidator(store);

        _leaderboardSource = leaderboardSource
            ?? throw new ArgumentNullException(nameof(leaderboardSource));
    }

    private static string NormalizeLanguage(string language) =>
        string.Equals(language, "es", StringComparison.OrdinalIgnoreCase) ? "es" : "en";
}
