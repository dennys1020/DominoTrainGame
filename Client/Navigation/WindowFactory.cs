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
    private readonly LocalizationViewModel _localization;
    private readonly IReadOnlyDictionary<Screen, Func<IWindowService, ScreenViewModel>> _viewModelsByScreen;
    private readonly IReadOnlyDictionary<Screen, Func<IWindowService, RoomRole, ScreenViewModel>>
        _roomViewModelsByScreen;

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

    public WindowFactory(IRemoteGameServices gameServices, LocalizationViewModel localization)
    {
        _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));

        _viewModelsByScreen = new Dictionary<Screen, Func<IWindowService, ScreenViewModel>>
        {
            [Screen.Login] = windowService => new LoginViewModel(windowService, _gameServices, _localization),
            [Screen.SignUp] = windowService => new SignUpViewModel(windowService, _gameServices, _localization),
            [Screen.Verification] = windowService => new VerificationCodeViewModel(windowService),
            [Screen.RecoverPassword] = windowService => new DialogViewModel(windowService),
            [Screen.Main] = windowService => new MainMenuViewModel(windowService),
            [Screen.NewGame] = windowService => new NewGameViewModel(windowService),
            [Screen.JoinGame] = windowService => new JoinGameViewModel(windowService),
            [Screen.Settings] = windowService => new DialogViewModel(windowService),
            [Screen.Rules] = windowService => new DialogViewModel(windowService),
            [Screen.Profile] = windowService => new DialogViewModel(windowService),
            [Screen.Friends] = windowService => new DialogViewModel(windowService),
            [Screen.ChangePassword] = windowService => new DialogViewModel(windowService),
            [Screen.DeleteAccount] = windowService => new DialogViewModel(windowService),
            [Screen.Leaderboard] = windowService => new DialogViewModel(windowService)
        };

        _roomViewModelsByScreen = new Dictionary<Screen, Func<IWindowService, RoomRole, ScreenViewModel>>
        {
            [Screen.Room] = (windowService, roomRole) => new RoomViewModel(windowService, roomRole),
            [Screen.MatchResults] = (windowService, roomRole) => new MatchResultsViewModel(windowService, roomRole)
        };
    }

    public Window Create(Screen screen)
    {
        bool hasViewModel = _viewModelsByScreen.TryGetValue(
            screen,
            out Func<IWindowService, ScreenViewModel> createViewModel);

        if (!hasViewModel)
        {
            throw new ArgumentOutOfRangeException(nameof(screen));
        }

        Window window = CreateWindow(screen);
        IWindowService windowService = new WindowService(window, this, _localization);
        window.DataContext = createViewModel(windowService);
        return window;
    }

    public Window Create(Screen screen, RoomRole roomRole)
    {
        bool hasViewModel = _roomViewModelsByScreen.TryGetValue(
            screen,
            out Func<IWindowService, RoomRole, ScreenViewModel> createViewModel);

        if (!hasViewModel)
        {
            throw new ArgumentOutOfRangeException(nameof(screen));
        }

        Window window = CreateWindow(screen);
        IWindowService windowService = new WindowService(window, this, _localization);
        window.DataContext = createViewModel(windowService, roomRole);
        return window;
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
