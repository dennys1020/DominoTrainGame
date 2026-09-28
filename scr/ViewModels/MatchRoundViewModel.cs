using System.Collections.ObjectModel;

namespace DominoTrainGame.ViewModels;

public sealed class MatchRoundViewModel
{
    public MatchRoundViewModel()
    {
        Scores = new ObservableCollection<RoundScoreViewModel>();
    }

    public ObservableCollection<RoundScoreViewModel> Scores
    {
        get;
    }
}
