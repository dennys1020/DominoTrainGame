using DominoTrainGame.Client.ViewModel.Navigation;

namespace DominoTrainGame.Client.ViewModel.Contracts;

public interface IWindowService
{
    void Navigate(Screen screen);

    void ShowDialog(Screen screen);

    void ShowMessage(string messageKey);

    bool ConfirmMessage(string messageKey);
    
    bool Confirm(string message);

    void Close();
}
