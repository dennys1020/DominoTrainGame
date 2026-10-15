using System.Windows;
using DominoTrainGame.Client.Models;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Navigation;
using DominoTrainGame.ViewModels;
using DominoTrainGame.Views;

namespace DominoTrainGame;

internal static class ClientComposition
{
    private static readonly IGameServices _services = new RemoteGameServices();

    public static void Bind(Window window, Screen screen, object context = null)
    {
        IWindowService navigation = new WindowService(window);
        window.DataContext = screen switch
        {
            Screen.Login => new LoginViewModel(_services, navigation),
            Screen.SignUp => new SignUpViewModel(_services, navigation),
            Screen.Leaderboard => new LeaderboardViewModel(_services),
            _ => new NavigationViewModel(navigation, screen)
        };
    }

    public static Window CreateWindow(Screen screen, object context = null) => screen switch
    {
        Screen.Login => new LogInWindow(),
        Screen.SignUp => new SignUpWindow(),
        Screen.Main => new MainWindow(),
        Screen.NewGame => new NewGameWindow(),
        Screen.JoinGame => new JoinGameWindow(),
        Screen.Room => new RoomWindow(),
        Screen.MatchResults => new MatchResultsWindow(),
        Screen.RecoverPassword => new RecoverPasswordWindow(),
        Screen.Settings => new SettingsWindow(),
        Screen.Rules => new GameRulesWindow(),
        Screen.Profile => new ProfileWindow(),
        Screen.Friends => new FriendsListWindow(),
        Screen.ChangePassword => new ChangePasswordWindow(),
        Screen.DeleteAccount => new DeleteAccountWindow(),
        Screen.Leaderboard => new LeaderBoardWindow(),
        _ => throw new System.ArgumentOutOfRangeException(nameof(screen))
    };
}
