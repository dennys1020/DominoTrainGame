using System.Collections.Generic;
using System.Windows;

namespace DominoTrainGame.Views
{
    public partial class ProfileWindow : Window
    {
        private bool _isEditing = false;

        private readonly List<string> _availableAvatars = new List<string>
        {
            "pack://application:,,,/Resources/Images/avatar_generic_preview.png",
        };

        public ProfileWindow()
        {
            InitializeComponent();
            LoadUserData();
            SetupAvatarGallery();
        }

        private void LoadUserData()
        {
        }

        private void SetupAvatarGallery()
        {
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditing)
            {
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ExitEditingMode();
            LoadUserData();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

            ExitEditingMode();
        }

        private void ExitEditingMode()
        {
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void InstagramButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        private void XButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO
        }

        private void FacebookButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }
    }
}
