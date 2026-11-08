using System;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Common;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Navigation;

namespace DominoTrainGame.Client.ViewModel;

public sealed class MainMenuViewModel : ScreenViewModel
{
    private readonly IRemoteGameServices _gameServices;
    private readonly IRemoteSocialServices _socialServices;
    private readonly AsyncRelayCommand _connectSocialCommand;

    public MainMenuViewModel(
        IWindowService windowService,
        IRemoteGameServices gameServices,
        IRemoteSocialServices socialServices) : base(windowService)
    {
        _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        _socialServices = socialServices ?? throw new ArgumentNullException(nameof(socialServices));

        NewGameCommand = new RelayCommand(() => WindowService.Navigate(Screen.NewGame));
        ResultsCommand = new RelayCommand(() => WindowService.Navigate(Screen.MatchResults));
        LogoutCommand = new RelayCommand(LogOut);
        LeaderboardCommand = new RelayCommand(() => WindowService.ShowDialog(Screen.Leaderboard));
        ProfileCommand = new RelayCommand(() => WindowService.ShowDialog(Screen.Profile));
        FriendsCommand = new RelayCommand(() => WindowService.ShowDialog(Screen.Friends));

        _connectSocialCommand = new AsyncRelayCommand(ConnectSocialAsync);
        _connectSocialCommand.Execute(null);
    }

    public RelayCommand NewGameCommand { get; }

    public RelayCommand ResultsCommand { get; }

    public RelayCommand LogoutCommand { get; }

    public RelayCommand LeaderboardCommand { get; }

    public RelayCommand ProfileCommand { get; }

    public RelayCommand FriendsCommand { get; }

    private async Task ConnectSocialAsync()
    {
        await RunWhileBusyAsync(() => _socialServices.ConnectAsync());
    }

    private void LogOut()
    {
        _socialServices.Disconnect();
        _gameServices.LogOut();
        WindowService.Navigate(Screen.Login);
    }
}
