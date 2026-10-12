using System.Windows;
using DominoTrainGame.ViewModels;

namespace DominoTrainGame.Views;

public partial class MatchResultsWindow : Window
{
    public MatchResultsWindow() : this(new RoomContext(false, new RoomViewModel())) { }
    public MatchResultsWindow(RoomViewModel room, bool isHost) : this(new RoomContext(isHost, room)) { }

    public MatchResultsWindow(RoomContext context)
    {
        InitializeComponent();
        ClientComposition.Bind(this, Screen.MatchResults, context);
    }
}
