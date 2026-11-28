using System.Windows.Controls;

namespace DominoTrainGame.Client.View.Views.Controls;

public sealed partial class LobbyChatPanel : UserControl
{
    private const double NoExtentChange = 0;

    public LobbyChatPanel()
    {
        InitializeComponent();
    }

    private void OnChatScrollViewerScrollChanged(object sender, ScrollChangedEventArgs eventArgs)
    {
        bool hasNewContent = eventArgs.ExtentHeightChange > NoExtentChange;

        if (hasNewContent)
        {
            ChatScrollViewer.ScrollToEnd();
        }
    }
}
