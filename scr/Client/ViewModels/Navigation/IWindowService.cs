namespace DominoTrainGame.ViewModels;

public enum Screen
{
    Login, SignUp, Main, NewGame, JoinGame, Room, MatchResults,
    RecoverPassword, Verification, Settings, Rules, Profile, Friends, ChangePassword, DeleteAccount, Leaderboard
}

public interface IWindowService
{
    bool IsVisible { get; }
    void Navigate(Screen screen, object context = null, bool preservePosition = true);
    bool? ShowDialog(Screen screen, object context = null);
    void ShowMessage(string message);
    void Close(bool? dialogResult = null);
}
