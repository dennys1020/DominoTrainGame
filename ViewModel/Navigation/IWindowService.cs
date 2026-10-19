namespace DominoTrainGame.ViewModels.Navigation;

public interface IWindowService
{
    void Navigate(Screen screen);

    void Navigate(Screen screen, RoomRole roomRole);

    void ShowDialog(Screen screen);

    void ShowMessage(string message);

    void Close();
}
