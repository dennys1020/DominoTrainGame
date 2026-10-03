using System.ComponentModel;

namespace DominoTrainGame.ViewModels;

public sealed class RoundScoreViewModel : INotifyPropertyChanged
{
    private int? _points;

    public RoundScoreViewModel(RoomPlayerViewModel player, int? points = null)
    {
        Player = player;
        _points = points;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public RoomPlayerViewModel Player
    {
        get;
    }

    public int? Points
    {
        get
        {
            return _points;
        }
        set
        {
            if (_points != value)
            {
                _points = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Points)));
            }
        }
    }
}
