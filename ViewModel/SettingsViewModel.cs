using DominoTrainGame.Client.ViewModel.Common;
using DominoTrainGame.Client.ViewModel.Contracts;

namespace DominoTrainGame.Client.ViewModel;

public sealed class SettingsViewModel : ScreenViewModel
{
    private const double DefaultMusicVolume = 70;
    private const double DefaultSoundEffectsVolume = 85;

    private double _musicVolume;
    private double _soundEffectsVolume;
    private bool _isFullScreenEnabled;

    public SettingsViewModel(IWindowService windowService) : base(windowService)
    {
        _musicVolume = DefaultMusicVolume;
        _soundEffectsVolume = DefaultSoundEffectsVolume;
    }

    public double MusicVolume
    {
        get
        {
            return _musicVolume;
        }
        set
        {
            SetProperty(ref _musicVolume, value);
        }
    }

    public double SoundEffectsVolume
    {
        get
        {
            return _soundEffectsVolume;
        }
        set
        {
            SetProperty(ref _soundEffectsVolume, value);
        }
    }

    public bool IsFullScreenEnabled
    {
        get
        {
            return _isFullScreenEnabled;
        }
        set
        {
            SetProperty(ref _isFullScreenEnabled, value);
        }
    }
}
