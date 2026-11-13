using System;
using System.Collections.Generic;
using System.Windows;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.View.Views;
using DominoTrainGame.Client.ViewModel;
using DominoTrainGame.Client.ViewModel.Common;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Localization;
using DominoTrainGame.Client.ViewModel.Navigation;

namespace DominoTrainGame.Client.View.Navigation;

internal sealed class WindowFactory
{
    private static readonly IReadOnlyDictionary<Screen, Func<Window>> _windowsByScreen;

    private readonly IRemoteGameServices _gameServices;
    private readonly IRemoteMatchServices _matchServices;
    private readonly IRemoteSocialServices _socialServices;
    private readonly LocalizationViewModel _localization;
    private readonly LobbyInvitationController _invitationController;
    private readonly IReadOnlyDictionary<Screen, Func<IWindowService, ScreenViewModel>> _viewModelsByScreen;

    static WindowFactory()
    {
        _windowsByScreen = new Dictionary<Screen, Func<Window>>
        {
            [Screen.Login] = () => new LogInWindow(),
            [Screen.SignUp] = () => new SignUpWindow(),
            [Screen.Verification] = () => new VerificationCodeWindow(),
            [Screen.RecoverPassword] = () => new RecoverPasswordWindow(),
            [Screen.Main] = () => new MainWindow(),
            [Screen.NewGame] = () => new NewGameWindow(),
            [Screen.JoinGame] = () => new JoinGameWindow(),
            [Screen.Room] = () => new RoomWindow(),
            [Screen.Match] = () => new MatchWindow(),
            [Screen.MatchResults] = () => new MatchResultsWindow(),
            [Screen.Settings] = () => new SettingsWindow(),
            [Screen.Rules] = () => new GameRulesWindow(),
            [Screen.Profile] = () => new ProfileWindow(),
            [Screen.Friends] = () => new FriendsListWindow(),
            [Screen.ChangePassword] = () => new ChangePasswordWindow(),
            [Screen.DeleteAccount] = () => new DeleteAccountWindow(),
            [Screen.Leaderboard] = () => new LeaderboardWindow()
        };
    }

    public WindowFactory(
        RemoteServices remoteServices,
        LocalizationViewModel localization)
    {
        if (remoteServices == null)
        {
            throw new ArgumentNullException(nameof(remoteServices));
        }

        _gameServices = remoteServices.GameServices;
        _socialServices = remoteServices.SocialServices;
        _matchServices = remoteServices.MatchServices;
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _invitationController = new LobbyInvitationController(_gameServices, _localization);

        _viewModelsByScreen = new Dictionary<Screen, Func<IWindowService, ScreenViewModel>>
        {
            [Screen.Login] = windowService => new LoginViewModel(windowService, _gameServices),
            [Screen.SignUp] = windowService => new SignUpViewModel(windowService, _gameServices),
            [Screen.Verification] = windowService => new VerificationCodeViewModel(windowService),
            [Screen.Main] = windowService => new MainMenuViewModel(
                windowService,
                _gameServices,
                _socialServices),
            [Screen.NewGame] = windowService => new NewGameViewModel(windowService, _gameServices),
            [Screen.JoinGame] = windowService => new JoinGameViewModel(windowService, _gameServices),
            [Screen.Room] = windowService => new RoomViewModel(
                windowService,
                _gameServices,
                new InviteFriendsViewModel(windowService, _gameServices, _socialServices),
                _localization),
            [Screen.Match] = windowService => new MatchViewModel(windowService, _gameServices, _matchServices),
            [Screen.MatchResults] = windowService => new MatchResultsViewModel(windowService, _gameServices),
            [Screen.Friends] = windowService => new FriendsViewModel(windowService, _socialServices, _localization)
        };
    }

    public Window Create(Screen screen)
    {
        Window window = CreateDialog(screen);
        _invitationController.Attach(new WindowService(window, this, _localization));
        return window;
    }

    public Window CreateDialog(Screen screen)
    {
        Window window = CreateWindow(screen);
        IWindowService windowService = new WindowService(window, this, _localization);
        window.DataContext = CreateViewModel(screen, windowService);
        window.Closed += (sender, eventArgs) => UnsubscribeViewModel(window.DataContext);
        return window;
    }

    private ScreenViewModel CreateViewModel(Screen screen, IWindowService windowService)
    {
        bool hasViewModel = _viewModelsByScreen.ContainsKey(screen);
        ScreenViewModel viewModel = hasViewModel
            ? _viewModelsByScreen[screen](windowService)
            : new DialogViewModel(windowService);

        return viewModel;
    }

    private static void UnsubscribeViewModel(object viewModel)
    {
        if (viewModel is ISubscribingViewModel subscribingViewModel)
        {
            subscribingViewModel.Unsubscribe();
        }
    }

    private static Window CreateWindow(Screen screen)
    {
        if (!_windowsByScreen.ContainsKey(screen))
        {
            throw new ArgumentOutOfRangeException(nameof(screen));
        }

        Func<Window> createWindow = _windowsByScreen[screen];
        Window window = createWindow();
        return window;
    }
}
