using System.Windows;

namespace DominoTrainGame.Views;

public partial class FriendsListWindow : Window
{
    public FriendsListWindow()
    {
        InitializeComponent();
    }

    private void OnAcceptRequestClicked(object sender, RoutedEventArgs e)
    {
        // TODO: Accept the selected request when the friendship service is defined.
    }

    private void OnAddFriendClicked(object sender, RoutedEventArgs e)
    {
        // TODO: Send a request to the selected player when the friendship service is defined.
    }

    private void OnRejectRequestClicked(object sender, RoutedEventArgs e)
    {
        // TODO: Reject the selected request when the friendship service is defined.
    }

    private void OnRemoveFriendClicked(object sender, RoutedEventArgs e)
    {
        // TODO: Remove the selected friend when the friendship service is defined.
    }

    private void OnTabChecked(object sender, RoutedEventArgs e)
    {
        // The Checked event can fire before both panels exist.
        if (FriendsPanel == null || RequestsPanel == null)
        {
            return;
        }

        if (FriendsTab.IsChecked == true)
        {
            FriendsPanel.Visibility = Visibility.Visible;
            RequestsPanel.Visibility = Visibility.Collapsed;
        }
        else if (RequestsTab.IsChecked == true)
        {
            FriendsPanel.Visibility = Visibility.Collapsed;
            RequestsPanel.Visibility = Visibility.Visible;
        }
    }

}
