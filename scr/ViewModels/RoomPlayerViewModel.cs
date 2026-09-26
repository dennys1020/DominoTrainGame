using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DominoTrainGame.ViewModels;

public sealed class RoomPlayerViewModel : INotifyPropertyChanged
{
    private string _username;
    private bool _isHost;
    private bool _isReady;
    private bool _isCurrentPlayer;

    public RoomPlayerViewModel(string playerId, string username)
    {
        PlayerId = playerId;
        _username = username;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public string PlayerId { get; }

    public string Username
    {
        get
        {
            return _username;
        }
        set
        {
            SetField(ref _username, value);
        }
    }

    public bool IsHost
    {
        get
        {
            return _isHost;
        }
        set
        {
            SetField(ref _isHost, value);
        }
    }

    public bool IsReady
    {
        get
        {
            return _isReady;
        }
        set
        {
            SetField(ref _isReady, value);
        }
    }

    public bool IsCurrentPlayer
    {
        get
        {
            return _isCurrentPlayer;
        }
        set
        {
            SetField(ref _isCurrentPlayer, value);
        }
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
