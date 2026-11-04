using System.Windows;
using System.Windows.Controls;

namespace DominoTrainGame.Client.View.Views;

public sealed partial class RoomWindow : Window
{
    private const double NoExtentChange = 0;

    public RoomWindow()
    {
        InitializeComponent();
    }

    private void OnChatScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        bool hasNewContent = e.ExtentHeightChange > NoExtentChange;

        if (hasNewContent)
        {
            ChatScrollViewer.ScrollToEnd();
        }
    }
}
