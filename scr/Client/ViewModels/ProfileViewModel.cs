namespace DominoTrainGame.ViewModels;

public sealed class ProfileViewModel : ScreenViewModel
{
    public ProfileViewModel(IWindowService window) : base(window)
    {
        // Profile editing and social links were unimplemented in the original view.
        EditCommand = new RelayCommand(() => { });
        CancelEditingCommand = new RelayCommand(() => { });
        SaveCommand = new RelayCommand(() => { });
        InstagramCommand = new RelayCommand(() => { });
        XCommand = new RelayCommand(() => { });
        FacebookCommand = new RelayCommand(() => { });
    }

    public RelayCommand EditCommand { get; }
    public RelayCommand CancelEditingCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand InstagramCommand { get; }
    public RelayCommand XCommand { get; }
    public RelayCommand FacebookCommand { get; }
}
