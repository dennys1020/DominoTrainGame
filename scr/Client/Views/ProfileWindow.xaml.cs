using System.Collections.Generic;
using System.Windows;

namespace DominoTrainGame.Views;

public partial class ProfileWindow : Window
{
    private bool _isEditing = false;

    private readonly List<string> _availableAvatars = new List<string>
    {
        "pack://application:,,,/Client/Resources/Images/avatar_generic_preview.png",
    };

    public ProfileWindow()
    {
        InitializeComponent();
        LoadUserData();
        SetupAvatarGallery();
    }

    private void LoadUserData()
    {
        // TODO: Load the current player's profile when the profile service is defined.
    }

    private void SetupAvatarGallery()
    {
        // TODO: Populate the avatar gallery from the available avatars.
    }

    private void OnEditButtonClicked(object sender, RoutedEventArgs e)
    {
        if (!_isEditing)
        {
            // TODO: Enable profile editing when the editable fields are defined.
        }
    }

    private void OnCancelButtonClicked(object sender, RoutedEventArgs e)
    {
        ExitEditingMode();
        LoadUserData();
    }

    private void OnSaveButtonClicked(object sender, RoutedEventArgs e)
    {
        // TODO: Save the edited profile when the profile service is defined.
        ExitEditingMode();
    }

    private void ExitEditingMode()
    {
        // TODO: Restore the profile controls when the editing behavior is defined.
    }

    private void OnCloseButtonClicked(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void OnInstagramButtonClicked(object sender, RoutedEventArgs e)
    {
        // TODO: Open the player's Instagram link when social profile links are defined.
    }

    private void OnXButtonClicked(object sender, RoutedEventArgs e)
    {
        // TODO: Open the player's X link when social profile links are defined.
    }

    private void OnFacebookButtonClicked(object sender, RoutedEventArgs e)
    {
        // TODO: Open the player's Facebook link when social profile links are defined.
    }
}
