using System.Windows;
using System.Windows.Controls;
using DominoTrainGame.Navigation;
using DominoTrainGame.ViewModels;

namespace DominoTrainGame.Views.Controls;

public partial class SettingsButton : UserControl
{
    public SettingsButton()
    {
        InitializeComponent();
        DataContext = new RelayCommand(() => new WindowService(Window.GetWindow(this)).ShowDialog(Screen.Settings));
    }
}
