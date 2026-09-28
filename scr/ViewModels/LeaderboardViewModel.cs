#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using DominoTrainGame.Models;
using DominoTrainGame.Repository;
using DominoTrainGame.Resources.Localization;
using log4net.Ext.Trace;

namespace DominoTrainGame.ViewModels;

public sealed class LeaderboardViewModel : INotifyPropertyChanged
{
    private static readonly ITraceLog _logger;

    private readonly LeaderboardRepository _repository = new LeaderboardRepository();
    private string _playerFilter = string.Empty;
    private bool _hasLoadError;

    static LeaderboardViewModel()
    {
        _logger = TraceLogManager.GetLogger(typeof(LeaderboardViewModel));
    }

    public LeaderboardViewModel()
    {
        Entries = CreateEntriesView(new List<LeaderboardEntry>());
        PropertyChangedEventManager.AddHandler(Localization, OnLanguageChanged, string.Empty);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public LocalizationViewModel Localization
    {
        get
        {
            return LocalizationViewModel.Instance;
        }
    }

    public ICollectionView Entries { get; private set; }

    public bool IsLoading { get; private set; }

    public bool CanRefresh
    {
        get
        {
            return !IsLoading;
        }
    }

    public string PlayerFilter
    {
        get
        {
            return _playerFilter;
        }
        set
        {
            _playerFilter = value ?? string.Empty;
            Entries.Refresh();
            NotifyStateChanged();
        }
    }

    public string StatusMessage
    {
        get
        {
            string message = string.Empty;
            if (IsLoading)
            {
                message = UiStrings.LeaderboardLoadingMessage;
            }
            else if (_hasLoadError)
            {
                message = UiStrings.LeaderboardLoadErrorMessage;
            }
            else if (Entries.IsEmpty)
            {
                message = string.IsNullOrWhiteSpace(PlayerFilter)
                    ? UiStrings.LeaderboardEmptyMessage
                    : UiStrings.LeaderboardNoMatchesMessage;
            }

            return message;
        }
    }

    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;
        _hasLoadError = false;
        Entries = CreateEntriesView(new List<LeaderboardEntry>());
        NotifyStateChanged();

        try
        {
            List<LeaderboardEntry> entries = await _repository.GetEntriesAsync(cancellationToken);
            Entries = CreateEntriesView(entries);
        }
        catch (EntityException exception)
        {
            HandleLoadError(exception);
        }
        catch (SqlException exception)
        {
            HandleLoadError(exception);
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    private ICollectionView CreateEntriesView(List<LeaderboardEntry> entries)
    {
        ListCollectionView entriesView = new ListCollectionView(entries)
        {
            Filter = MatchesPlayerFilter
        };
        return entriesView;
    }

    private bool MatchesPlayerFilter(object item)
    {
        LeaderboardEntry entry = (LeaderboardEntry)item;
        bool matchesFilter = entry.Username.IndexOf(
            _playerFilter.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
        return matchesFilter;
    }

    private void HandleLoadError(Exception exception)
    {
        _logger.Error("The leaderboard could not be loaded from the game database.", exception);
        _hasLoadError = true;
    }

    private void OnLanguageChanged(object? sender, PropertyChangedEventArgs eventArgs)
    {
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
    }
}
