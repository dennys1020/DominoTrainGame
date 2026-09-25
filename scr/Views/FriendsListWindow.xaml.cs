using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DominoTrainGame.Views
{
    /// <summary>
    /// Lógica de interacción para FriendsList.xaml
    /// </summary>
    public partial class FriendsListWindow : Window
    {
        public FriendsListWindow()
        {
            InitializeComponent();
        }

        private void AcceptRequest_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddFriend_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RejectRequest_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveFriend_Click(object sender, RoutedEventArgs e)
        {

        }

        //FIX 
        private void Tab_Checked(object sender, RoutedEventArgs e)
        {
            if (FriendsPanel == null || RequestsPanel == null) return;

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
}
