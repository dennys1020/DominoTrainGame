using System.Windows;
using DominoTrainGame.ViewModels;

namespace DominoTrainGame.Views;

public partial class RoomWindow : Window
{
    public RoomWindow() : this(new RoomContext(false)) { }
    public RoomWindow(bool isHost) : this(new RoomContext(isHost)) { }
    public RoomWindow(RoomViewModel room, bool isHost) : this(new RoomContext(isHost, room)) { }

    public RoomWindow(RoomContext context)
    {
        InitializeComponent();
        ClientComposition.Bind(this, Screen.Room, context);
    }
}
