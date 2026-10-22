using DominoTrainGame.Client.ViewModel.Navigation;

namespace DominoTrainGame.Client.ViewModel.Contracts;

public interface IWindowService
{
    void Navigate(Screen screen);

    void Navigate(Screen screen, RoomRole roomRole);

    void ShowDialog(Screen screen);

    void ShowMessage(string message);

    void Close();
}
