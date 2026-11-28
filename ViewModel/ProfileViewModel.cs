using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Common;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Localization;
using DominoTrainGame.Client.ViewModel.Navigation;

namespace DominoTrainGame.Client.ViewModel;

public sealed class ProfileViewModel : ScreenViewModel
{
    private const string ProfileSavedMessageKey = "ProfileSavedMessage";
    private const string AvatarUpdateMessageKey = "ProfileAvatarUpdateMessage";
    private const string SocialLinkMissingMessageKey = "ProfileSocialLinkMissingMessage";
    private const string InstagramAddressFormat = "https://www.instagram.com/{0}";
    private const string FacebookAddressFormat = "https://www.facebook.com/{0}";
    private const string XAddressFormat = "https://x.com/{0}";
    private const string MemberSinceDateFormat = "d";

    private readonly IRemoteProfileServices _profileServices;
    private readonly ProfileOwner _profileOwner;
    private PlayerProfile _loadedProfile;
    private string _username;
    private string _biography;
    private string _instagramUsername;
    private string _facebookUsername;
    private string _xUsername;
    private string _memberSince;
    private byte[] _avatarImage;
    private int _gamesPlayed;
    private int _wins;
    private int _totalPoints;
    private bool _isEditing;

    public ProfileViewModel(
        IWindowService windowService,
        IRemoteProfileServices profileServices,
        ProfileOwner profileOwner) : base(windowService)
    {
        _profileServices = profileServices ?? throw new ArgumentNullException(nameof(profileServices));
        _profileOwner = profileOwner ?? throw new ArgumentNullException(nameof(profileOwner));
        _loadedProfile = PlayerProfile.Empty;
        _username = string.Empty;
        _biography = string.Empty;
        _instagramUsername = string.Empty;
        _facebookUsername = string.Empty;
        _xUsername = string.Empty;
        _memberSince = string.Empty;
        _avatarImage = Array.Empty<byte>();

        EditCommand = new RelayCommand(StartEditingProfile);
        CancelEditingCommand = new RelayCommand(CancelEditingProfile);
        SaveCommand = new AsyncRelayCommand(SaveProfileChangesAsync);
        ChangeAvatarCommand = new AsyncRelayCommand(ChangeAvatarImageAsync);
        InstagramCommand = new RelayCommand(OpenInstagramProfile);
        FacebookCommand = new RelayCommand(OpenFacebookProfile);
        XCommand = new RelayCommand(OpenXProfile);
        RefreshCommand = new AsyncRelayCommand(LoadProfileAsync);
        RefreshCommand.Execute(null);
    }

    public bool CanEditProfile => _profileOwner.IsSignedInPlayer;

    public bool IsEditing
    {
        get
        {
            return _isEditing;
        }
        private set
        {
            bool hasChanged = SetProperty(ref _isEditing, value);

            if (hasChanged)
            {
                OnPropertyChanged(nameof(IsViewing));
                OnPropertyChanged(nameof(IsEditButtonVisible));
            }
        }
    }

    public bool IsViewing => !IsEditing;

    public bool IsEditButtonVisible => CanEditProfile && !IsEditing;

    public string Username
    {
        get
        {
            return _username;
        }
        set
        {
            SetProperty(ref _username, value ?? string.Empty);
        }
    }

    public string Biography
    {
        get
        {
            return _biography;
        }
        set
        {
            SetProperty(ref _biography, value ?? string.Empty);
        }
    }

    public string InstagramUsername
    {
        get
        {
            return _instagramUsername;
        }
        set
        {
            SetProperty(ref _instagramUsername, value ?? string.Empty);
        }
    }

    public string FacebookUsername
    {
        get
        {
            return _facebookUsername;
        }
        set
        {
            SetProperty(ref _facebookUsername, value ?? string.Empty);
        }
    }

    public string XUsername
    {
        get
        {
            return _xUsername;
        }
        set
        {
            SetProperty(ref _xUsername, value ?? string.Empty);
        }
    }

    public string MemberSince
    {
        get
        {
            return _memberSince;
        }
        private set
        {
            SetProperty(ref _memberSince, value);
        }
    }

    public byte[] AvatarImage
    {
        get
        {
            return _avatarImage;
        }
        private set
        {
            bool hasChanged = SetProperty(ref _avatarImage, value);

            if (hasChanged)
            {
                OnPropertyChanged(nameof(HasAvatarImage));
            }
        }
    }

    public bool HasAvatarImage => _avatarImage.Any();

    public int GamesPlayed
    {
        get
        {
            return _gamesPlayed;
        }
        private set
        {
            SetProperty(ref _gamesPlayed, value);
        }
    }

    public int Wins
    {
        get
        {
            return _wins;
        }
        private set
        {
            SetProperty(ref _wins, value);
        }
    }

    public int TotalPoints
    {
        get
        {
            return _totalPoints;
        }
        private set
        {
            SetProperty(ref _totalPoints, value);
        }
    }

    public RelayCommand EditCommand { get; }

    public RelayCommand CancelEditingCommand { get; }

    public AsyncRelayCommand SaveCommand { get; }

    public AsyncRelayCommand ChangeAvatarCommand { get; }

    public RelayCommand InstagramCommand { get; }

    public RelayCommand FacebookCommand { get; }

    public RelayCommand XCommand { get; }

    public AsyncRelayCommand RefreshCommand { get; }

    private async Task LoadProfileAsync()
    {
        string profileUsername = _profileOwner.IsSignedInPlayer
            ? _profileServices.CurrentUsername
            : _profileOwner.Username;
        PlayerProfileResult profileResult = await RunWhileBusyAsync(
            _profileServices.GetProfileAsync,
            profileUsername);

        if (profileResult.Status == ProfileActionResult.Success)
        {
            ShowLoadedProfile(profileResult.Profile);
        }
        else
        {
            WindowService.ShowMessageError(ProfileMessageKeys.GetMessageKey(profileResult.Status));
        }
    }

    private void ShowLoadedProfile(PlayerProfile profile)
    {
        _loadedProfile = profile;
        AvatarImage = profile.AvatarImage;
        MemberSince = profile.MemberSince.ToString(MemberSinceDateFormat, CultureInfo.CurrentUICulture);
        GamesPlayed = profile.Statistics.GamesPlayed;
        Wins = profile.Statistics.Wins;
        TotalPoints = profile.Statistics.TotalPoints;
        RestoreEditableFields();
    }

    private void RestoreEditableFields()
    {
        Username = _loadedProfile.Username;
        Biography = _loadedProfile.Biography;
        InstagramUsername = _loadedProfile.SocialLinks.InstagramUsername;
        FacebookUsername = _loadedProfile.SocialLinks.FacebookUsername;
        XUsername = _loadedProfile.SocialLinks.TwitterUsername;
    }

    private void StartEditingProfile()
    {
        IsEditing = CanEditProfile;
    }

    private void CancelEditingProfile()
    {
        RestoreEditableFields();
        IsEditing = false;
    }

    private async Task SaveProfileChangesAsync()
    {
        ProfileActionResult saveResult = await RunWhileBusyAsync(SendProfileChangesAsync);

        if (saveResult == ProfileActionResult.Success)
        {
            IsEditing = false;
            WindowService.ShowMessage(ProfileSavedMessageKey);
            await LoadProfileAsync();
        }
        else
        {
            WindowService.ShowMessageError(ProfileMessageKeys.GetMessageKey(saveResult));
        }
    }

    private async Task<ProfileActionResult> SendProfileChangesAsync()
    {
        ProfileActionResult saveResult = ProfileActionResult.Success;
        bool hasUsernameChanged = !string.Equals(
            Username,
            _profileServices.CurrentUsername,
            StringComparison.Ordinal);

        if (hasUsernameChanged)
        {
            saveResult = await _profileServices.ChangeUsernameAsync(Username);
        }

        if (saveResult == ProfileActionResult.Success)
        {
            saveResult = await _profileServices.UpdateBiographyAsync(Biography);
        }

        if (saveResult == ProfileActionResult.Success)
        {
            ProfileSocialLinks socialLinks = new ProfileSocialLinks(
                InstagramUsername,
                FacebookUsername,
                XUsername);
            saveResult = await _profileServices.UpdateSocialLinksAsync(socialLinks);
        }

        return saveResult;
    }

    private async Task ChangeAvatarImageAsync()
    {
        byte[] imageBytes = WindowService.SelectAvatarImageFile();
        bool hasSelectedImage = imageBytes.Any();

        if (hasSelectedImage)
        {
            ProfileActionResult avatarResult = await RunWhileBusyAsync(
                _profileServices.UpdateAvatarAsync,
                imageBytes);
            ShowAvatarUpdateResult(avatarResult, imageBytes);
        }
    }

    private void ShowAvatarUpdateResult(ProfileActionResult avatarResult, byte[] imageBytes)
    {
        if (avatarResult == ProfileActionResult.Success)
        {
            AvatarImage = imageBytes;
            WindowService.ShowMessage(AvatarUpdateMessageKey);
        }
        else
        {
            WindowService.ShowMessageError(ProfileMessageKeys.GetAvatarMessageKey(avatarResult));
        }
    }

    private void OpenInstagramProfile()
    {
        OpenSocialProfile(InstagramAddressFormat, InstagramUsername);
    }

    private void OpenFacebookProfile()
    {
        OpenSocialProfile(FacebookAddressFormat, FacebookUsername);
    }

    private void OpenXProfile()
    {
        OpenSocialProfile(XAddressFormat, XUsername);
    }

    private void OpenSocialProfile(string addressFormat, string socialUsername)
    {
        bool hasSocialUsername = !string.IsNullOrWhiteSpace(socialUsername);

        if (hasSocialUsername)
        {
            string socialAddress = string.Format(
                CultureInfo.InvariantCulture,
                addressFormat,
                Uri.EscapeDataString(socialUsername.Trim()));
            WindowService.OpenWebPage(socialAddress);
        }
        else
        {
            WindowService.ShowMessage(SocialLinkMissingMessageKey);
        }
    }
}
