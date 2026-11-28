using DominoTrainGame.Client.ViewModel.Navigation;

namespace DominoTrainGame.Client.ViewModel.Contracts;

public interface IWindowService
{
    void Navigate(Screen screen);

    void ShowDialog(Screen screen);

    void ShowPlayerProfile(string username);

    void ShowMessage(string messageKey);

    void ShowMessageError(string messageKey);

    bool ConfirmMessage(string messageKey);
    
    bool Confirm(string message);

    byte[] SelectAvatarImageFile();

    void OpenWebPage(string address);

    void Close();
}
