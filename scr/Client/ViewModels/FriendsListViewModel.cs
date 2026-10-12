namespace DominoTrainGame.ViewModels;

public sealed class FriendsListViewModel : ScreenViewModel
{
    private bool _isFriendsSelected = true;

    public FriendsListViewModel(IWindowService window) : base(window)
    {
        // These actions were placeholders in the original view.
        AcceptRequestCommand = new RelayCommand(() => { });
        AddFriendCommand = new RelayCommand(() => { });
        RejectRequestCommand = new RelayCommand(() => { });
        RemoveFriendCommand = new RelayCommand(() => { });
    }

    public bool IsFriendsSelected
    {
        get => _isFriendsSelected;
        set { SetField(ref _isFriendsSelected, value); Notify(nameof(IsRequestsSelected)); }
    }
    public bool IsRequestsSelected { get => !IsFriendsSelected; set => IsFriendsSelected = !value; }
    public RelayCommand AcceptRequestCommand { get; }
    public RelayCommand AddFriendCommand { get; }
    public RelayCommand RejectRequestCommand { get; }
    public RelayCommand RemoveFriendCommand { get; }
}
