using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using DominoTrainGame.Resources.Localization;

namespace DominoTrainGame.ViewModels;

public sealed class MatchResultsViewModel : INotifyPropertyChanged
{
    private const int FirstRoundNumber = 1;
    private const int SingleWinnerCount = 1;
    private const string PlayerNameSeparator = ", ";

    private readonly ObservableCollection<RoomPlayerViewModel> _connectedPlayers;
    private readonly List<MatchRoundViewModel> _observedRounds = new List<MatchRoundViewModel>();
    private readonly List<INotifyPropertyChanged> _observedValues = new List<INotifyPropertyChanged>();

    public MatchResultsViewModel(ObservableCollection<RoomPlayerViewModel> connectedPlayers)
    {
        Rounds = new ObservableCollection<MatchRoundViewModel>();
        _connectedPlayers = connectedPlayers;
        _connectedPlayers.CollectionChanged += OnCollectionChanged;
        Rounds.CollectionChanged += OnCollectionChanged;
        PropertyChangedEventManager.AddHandler(LocalizationViewModel.Instance, OnLanguageChanged, string.Empty);
        RefreshResults();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<MatchRoundViewModel> Rounds
    {
        get;
    }

    public IReadOnlyList<PlayerMatchResultViewModel> Players
    {
        get;
        private set;
    }

    public IReadOnlyList<string> RoundLabels
    {
        get;
        private set;
    }

    public string WinnerText
    {
        get;
        private set;
    }

    public bool HasWinner
    {
        get;
        private set;
    }

    public bool HasPlayers
    {
        get
        {
            bool hasPlayers = Players.Any();
            return hasPlayers;
        }
    }

    public string ConnectedPlayersText
    {
        get
        {
            string connectedPlayers = string.Format(
                UiStrings.Culture,
                UiStrings.ScoreConnectedPlayersFormat,
                _connectedPlayers.Count
            );
            return connectedPlayers;
        }
    }

    public string RoundCountText
    {
        get
        {
            string roundCount = string.Format(UiStrings.Culture, UiStrings.ScoreRoundCountFormat, Rounds.Count);
            return roundCount;
        }
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshResults();
    }

    private void OnValueChanged(object sender, PropertyChangedEventArgs e)
    {
        bool hasPlayerDisplayChanged = e.PropertyName == nameof(RoomPlayerViewModel.Username)
            || e.PropertyName == nameof(RoomPlayerViewModel.IsCurrentPlayer);
        bool hasRelevantChange = sender is RoundScoreViewModel || string.IsNullOrEmpty(e.PropertyName)
            || hasPlayerDisplayChanged;

        if (hasRelevantChange)
        {
            RefreshResults();
        }
    }

    private void OnLanguageChanged(object sender, PropertyChangedEventArgs e)
    {
        RefreshResults();
    }

    private void RefreshResults()
    {
        List<RoomPlayerViewModel> participants = _connectedPlayers
            .Concat(Rounds.SelectMany(round => round.Scores).Select(score => score.Player))
            .GroupBy(player => player.PlayerId)
            .Select(group => group.First())
            .ToList();

        ObserveSources(participants);
        BuildRows(participants);
        UpdateWinner();
        NotifyResultsChanged();
    }

    private void ObserveSources(IEnumerable<RoomPlayerViewModel> participants)
    {
        foreach (MatchRoundViewModel round in _observedRounds)
        {
            CollectionChangedEventManager.RemoveHandler(round.Scores, OnCollectionChanged);
        }

        foreach (INotifyPropertyChanged value in _observedValues)
        {
            PropertyChangedEventManager.RemoveHandler(value, OnValueChanged, string.Empty);
        }

        _observedRounds.Clear();
        _observedValues.Clear();
        _observedRounds.AddRange(Rounds.Distinct());
        _observedValues.AddRange(participants);
        _observedValues.AddRange(Rounds.SelectMany(round => round.Scores).Distinct());

        foreach (MatchRoundViewModel round in _observedRounds)
        {
            CollectionChangedEventManager.AddHandler(round.Scores, OnCollectionChanged);
        }

        foreach (INotifyPropertyChanged value in _observedValues)
        {
            PropertyChangedEventManager.AddHandler(value, OnValueChanged, string.Empty);
        }
    }

    private void BuildRows(IEnumerable<RoomPlayerViewModel> participants)
    {
        List<PlayerMatchResultViewModel> rows = new List<PlayerMatchResultViewModel>();

        foreach (RoomPlayerViewModel player in participants)
        {
            List<RoundScoreViewModel> scores = Rounds
                .Select(round => round.Scores.FirstOrDefault(score => score.Player.PlayerId == player.PlayerId)
                    ?? new RoundScoreViewModel(player))
                .ToList();
            rows.Add(new PlayerMatchResultViewModel(player, scores));
        }

        Players = rows;
        RoundLabels = Enumerable.Range(FirstRoundNumber, Rounds.Count)
            .Select(number => string.Format(UiStrings.Culture, UiStrings.ScoreRoundFormat, number))
            .ToList();
    }

    private void UpdateWinner()
    {
        HasWinner = Players.Any() && Players.All(player => player.TotalScore.HasValue);
        WinnerText = UiStrings.ScorePendingResultsLabel;

        if (HasWinner)
        {
            int? lowestScore = Players.Min(player => player.TotalScore);
            List<PlayerMatchResultViewModel> winners = Players
                .Where(player => player.TotalScore == lowestScore)
                .ToList();

            foreach (PlayerMatchResultViewModel winner in winners)
            {
                winner.IsWinner = true;
            }

            string names = string.Join(PlayerNameSeparator, winners.Select(winner =>
                winner.Player.IsCurrentPlayer ? UiStrings.YouPlayerLabel : winner.Player.Username));
            string format = winners.Count == SingleWinnerCount
                ? UiStrings.ScoreWinnerFormat
                : UiStrings.ScoreTieFormat;
            WinnerText = string.Format(UiStrings.Culture ?? CultureInfo.CurrentCulture, format, names);
        }
    }

    private void NotifyResultsChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Players)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RoundLabels)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConnectedPlayersText)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RoundCountText)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WinnerText)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasWinner)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasPlayers)));
    }
}
