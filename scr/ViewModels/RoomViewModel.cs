using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DominoTrainGame.ViewModels;

public sealed class RoomViewModel : INotifyPropertyChanged
{
    private const int DefaultMaximumPlayers = 4;
    private const int DoubleSix = 6;
    private const int DoubleNine = 9;
    private const int DoubleTwelve = 12;
    private const int DefaultMaximumDouble = DoubleTwelve;

    private int _maximumPlayers = DefaultMaximumPlayers;
    private int _maximumDouble = DefaultMaximumDouble;

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<RoomPlayerViewModel> Players { get; } =
        new ObservableCollection<RoomPlayerViewModel>();

    public int MaximumDouble
    {
        get
        {
            return _maximumDouble;
        }
        set
        {
            if (_maximumDouble != value)
            {
                _maximumDouble = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaximumDouble)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDoubleSixSelected)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDoubleNineSelected)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDoubleTwelveSelected)));
            }
        }
    }

    public bool IsDoubleSixSelected
    {
        get
        {
            return MaximumDouble == DoubleSix;
        }
        set
        {
            if (value)
            {
                MaximumDouble = DoubleSix;
            }
        }
    }

    public bool IsDoubleNineSelected
    {
        get
        {
            return MaximumDouble == DoubleNine;
        }
        set
        {
            if (value)
            {
                MaximumDouble = DoubleNine;
            }
        }
    }

    public bool IsDoubleTwelveSelected
    {
        get
        {
            return MaximumDouble == DoubleTwelve;
        }
        set
        {
            if (value)
            {
                MaximumDouble = DoubleTwelve;
            }
        }
    }

    public int MaximumPlayers
    {
        get
        {
            return _maximumPlayers;
        }
        set
        {
            if (_maximumPlayers != value)
            {
                _maximumPlayers = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaximumPlayers)));
            }
        }
    }
}
