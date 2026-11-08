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
    private readonly IRemoteSocialServices _socialServices;
    private readonly LocalizationViewModel _localization;
    private readonly LobbyInvitationController _invitationController;
    private readonly RoomDependencies _roomDependencies;
    private readonly IReadOnlyDictionary<Screen, Func<IWindowService, ScreenViewModel>> _viewModelsByScreen;
    private readonly IReadOnlyDictionary<Screen, Func<IWindowService, RoomRole, ScreenViewModel>>
        _roleViewModelsByScreen;

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
        IRemoteGameServices gameServices,
        IRemoteSocialServices socialServices,
        LocalizationViewModel localization)
    {
        _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        _socialServices = socialServices ?? throw new ArgumentNullException(nameof(socialServices));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _invitationController = new LobbyInvitationController(_gameServices, _localization);
        _roomDependencies = new RoomDependencies(_gameServices, _socialServices, _localization);

        _viewModelsByScreen = new Dictionary<Screen, Func<IWindowService, ScreenViewModel>>
        {
            [Screen.Login] = windowService => new LoginViewModel(windowService, _gameServices, _localization),
            [Screen.SignUp] = windowService => new SignUpViewModel(windowService, _gameServices, _localization),
            [Screen.Verification] = windowService => new VerificationCodeViewModel(windowService),
            [Screen.RecoverPassword] = windowService => new DialogViewModel(windowService),
            [Screen.Main] = windowService => new MainMenuViewModel(
                windowService,
                _gameServices,
                _socialServices),
            [Screen.NewGame] = windowService => new NewGameViewModel(
                windowService,
                _gameServices,
                _localization),
            [Screen.JoinGame] = windowService => new JoinGameViewModel(
                windowService,
                _gameServices,
                _localization),
            [Screen.Room] = windowService => new RoomViewModel(windowService, _roomDependencies),
            [Screen.Settings] = windowService => new DialogViewModel(windowService),
            [Screen.Rules] = windowService => new DialogViewModel(windowService),
            [Screen.Profile] = windowService => new DialogViewModel(windowService),
            [Screen.Friends] = windowService => new FriendsViewModel(windowService, _socialServices, _localization),
            [Screen.ChangePassword] = windowService => new DialogViewModel(windowService),
            [Screen.DeleteAccount] = windowService => new DialogViewModel(windowService),
            [Screen.Leaderboard] = windowService => new DialogViewModel(windowService)
        };

        _roleViewModelsByScreen = new Dictionary<Screen, Func<IWindowService, RoomRole, ScreenViewModel>>
        {
            [Screen.Match] = (windowService, roomRole) => new MatchViewModel(windowService, roomRole),
            [Screen.MatchResults] = (windowService, roomRole) => new MatchResultsViewModel(windowService, roomRole)
        };
    }

    public Window Create(Screen screen)
    {
        Window window = CreateWindow(screen);
        IWindowService windowService = new WindowService(window, this, _localization);
        window.DataContext = CreateViewModel(screen, windowService);
        window.Closed += (sender, eventArgs) => UnsubscribeViewModel(window.DataContext);
        _invitationController.Attach(windowService);
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

    public Window Create(Screen screen, RoomRole roomRole)
    {
        bool hasViewModel = _roleViewModelsByScreen.TryGetValue(
            screen,
            out Func<IWindowService, RoomRole, ScreenViewModel> createViewModel);

        if (!hasViewModel)
        {
            throw new ArgumentOutOfRangeException(nameof(screen));
        }

        Window window = CreateWindow(screen);
        IWindowService windowService = new WindowService(window, this, _localization);
        window.DataContext = createViewModel(windowService, roomRole);
        window.Closed += (sender, eventArgs) => UnsubscribeViewModel(window.DataContext);
        _invitationController.Attach(windowService);
        return window;
    }

    private ScreenViewModel CreateViewModel(Screen screen, IWindowService windowService)
    {
        bool hasViewModel = _viewModelsByScreen.TryGetValue(
            screen,
            out Func<IWindowService, ScreenViewModel> createViewModel);

        if (!hasViewModel)
        {
            throw new ArgumentOutOfRangeException(nameof(screen));
        }

        ScreenViewModel viewModel = createViewModel(windowService);
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
        bool hasWindow = _windowsByScreen.TryGetValue(screen, out Func<Window> createWindow);

        if (!hasWindow)
        {
            throw new ArgumentOutOfRangeException(nameof(screen));
        }

        Window window = createWindow();
        return window;
    }
}
