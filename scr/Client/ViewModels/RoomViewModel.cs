using System.Collections.Generic;

namespace DominoTrainGame.ViewModels;

// Display state only; room management and gameplay are outside the current scope.
public sealed class RoomViewModel : ObservableViewModel
{
    private int _maximumDouble = 12;

    public IReadOnlyList<object> Players { get; } = new List<object>();
    public int MaximumDouble
    {
        get => _maximumDouble;
        set
        {
            if (SetField(ref _maximumDouble, value))
            {
                Notify(nameof(IsDoubleSixSelected));
                Notify(nameof(IsDoubleNineSelected));
                Notify(nameof(IsDoubleTwelveSelected));
            }
        }
    }
    public bool IsDoubleSixSelected { get => MaximumDouble == 6; set { if (value) { MaximumDouble = 6; } } }
    public bool IsDoubleNineSelected { get => MaximumDouble == 9; set { if (value) { MaximumDouble = 9; } } }
    public bool IsDoubleTwelveSelected { get => MaximumDouble == 12; set { if (value) { MaximumDouble = 12; } } }
}
