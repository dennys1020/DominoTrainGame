using System;
using System.Globalization;
using DominoTrainGame.Resources.Localization;

namespace DominoTrainGame.ViewModels;

// Empty display state for the preserved results window.
public sealed class MatchResultsViewModel
{
    public object[] Players { get; } = Array.Empty<object>();
    public string[] RoundLabels { get; } = Array.Empty<string>();
    public bool HasPlayers => false;
    public bool HasWinner => false;
    public string WinnerText => UiStrings.ScorePendingResultsLabel.ToUpper(CultureInfo.CurrentUICulture);
    public string RoundCountText => string.Format(UiStrings.Culture, UiStrings.ScoreRoundCountFormat, 0);
    public string ConnectedPlayersText => string.Format(UiStrings.Culture, UiStrings.ScoreConnectedPlayersFormat, 0);
}
