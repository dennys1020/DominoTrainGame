using DominoTrainGame.Client.Models.Errors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using log4net.Ext.Trace;
using DominoTrainGame.Client.Models.DominoTrainService;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Resources.Localization;
#nullable enable

namespace DominoTrainGame.ViewModels;

public sealed class LeaderboardViewModel : INotifyPropertyChanged, IDisposable
{
    private static readonly ITraceLog _logger;

    private readonly IGameServices _services;
    private readonly CancellationTokenSource _loadCancellation = new CancellationTokenSource();
    private string _playerFilter = string.Empty;
    private bool _hasLoadError;

    static LeaderboardViewModel()
    {
        _logger = TraceLogManager.GetLogger(typeof(LeaderboardViewModel));
    }

    public LeaderboardViewModel(IGameServices services)
    {
        _services = services;
        RefreshCommand = new AsyncCommand(LoadAsync);
        Entries = CreateEntriesView(new List<LeaderboardEntry>());
        PropertyChangedEventManager.AddHandler(Localization, OnLanguageChanged, string.Empty);
    }

    public AsyncCommand RefreshCommand { get; }

    public async Task LoadAsync()
    {
        CancellationToken token = _loadCancellation.Token;
        try { await LoadAsync(token); }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
    }

    public void Dispose()
    {
        _loadCancellation.Cancel();
        _loadCancellation.Dispose();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public LocalizationViewModel Localization
    {
        get
        {
            return LocalizationViewModel.Instance;
        }
    }

    public ICollectionView Entries
    {
        get;
        private set;
    }

    public bool IsLoading
    {
        get;
        private set;
    }

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
            List<LeaderboardEntry> entries = await _services.GetEntriesAsync(cancellationToken);
            Entries = CreateEntriesView(entries);
        }
        catch (GameServiceException exception)
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
