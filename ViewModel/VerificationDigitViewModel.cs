using DominoTrainGame.Client.ViewModel.Common;

namespace DominoTrainGame.Client.ViewModel;

public sealed class VerificationDigitViewModel : ObservableViewModel
{
    private string _text = string.Empty;

    public string Text
    {
        get
        {
            return _text;
        }
        set
        {
            SetProperty(ref _text, value ?? string.Empty);
        }
    }
}
