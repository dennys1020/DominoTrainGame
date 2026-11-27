using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Common;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Localization;
using DominoTrainGame.Shared.Common;

namespace DominoTrainGame.Client.ViewModel;

public sealed class LeaderboardViewModel : ScreenViewModel
{
    private readonly IRemoteMatchServices _matchServices;
    private readonly List<LeaderboardEntryResult> _leaderboard = new List<LeaderboardEntryResult>();

    private string _playerFilter = string.Empty;
    private bool _hasNoEntries;

    public LeaderboardViewModel(IWindowService windowService, IRemoteMatchServices matchServices)
        : base(windowService)
    {
        _matchServices = matchServices ?? throw new ArgumentNullException(nameof(matchServices));
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        RefreshCommand.Execute(null);
    }

    public ObservableCollection<LeaderboardEntryResult> Entries { get; } =
        new ObservableCollection<LeaderboardEntryResult>();

    public string PlayerFilter
    {
        get
        {
            return _playerFilter;
        }
        set
        {
            bool hasChanged = SetProperty(ref _playerFilter, value ?? string.Empty);

            if (hasChanged)
            {
                ApplyFilter();
            }
        }
    }

    public bool HasNoEntries
    {
        get
        {
            return _hasNoEntries;
        }
        private set
        {
            SetProperty(ref _hasNoEntries, value);
        }
    }

    public AsyncRelayCommand RefreshCommand { get; }

    private async Task RefreshAsync()
    {
        LeaderboardResult leaderboardResult = await RunWhileBusyAsync(_matchServices.GetLeaderboardAsync);

        if (leaderboardResult.Status == LeaderboardStatus.Success)
        {
            _leaderboard.Clear();
            _leaderboard.AddRange(leaderboardResult.Entries);
            ApplyFilter();
        }
        else
        {
            WindowService.ShowMessageError(LeaderboardMessageKeys.GetMessageKey(leaderboardResult.Status));
        }
    }

    private void ApplyFilter()
    {
        Entries.Clear();

        foreach (LeaderboardEntryResult entry in _leaderboard)
        {
            if (MatchesFilter(entry))
            {
                Entries.Add(entry);
            }
        }

        HasNoEntries = !Entries.Any();
    }

    private bool MatchesFilter(LeaderboardEntryResult entry)
    {
        int filterIndex = entry.Username.IndexOf(_playerFilter.Trim(), StringComparison.OrdinalIgnoreCase);
        bool matchesFilter = CollectionIndex.IsFound(filterIndex);

        return matchesFilter;
    }
}
