using System.Collections.Generic;
using DominoTrainGame.Client.Models.Contracts;

namespace DominoTrainGame.Client.ViewModel.Localization;

public static class FriendStatusLabelKeys
{
    private const string OfflineLabelKey = "FriendStatusOfflineLabel";
    private const string OnlineLabelKey = "FriendStatusOnlineLabel";
    private const string InLobbyLabelKey = "FriendStatusInLobbyLabel";
    private const string InMatchLabelKey = "FriendStatusInMatchLabel";

    private static readonly IReadOnlyDictionary<FriendStatus, string> _labelKeysByStatus;

    static FriendStatusLabelKeys()
    {
        _labelKeysByStatus = new Dictionary<FriendStatus, string>
        {
            [FriendStatus.Online] = OnlineLabelKey,
            [FriendStatus.InLobby] = InLobbyLabelKey,
            [FriendStatus.InMatch] = InMatchLabelKey
        };
    }

    public static string GetLabelKey(FriendStatus status)
    {
        bool hasLabelKey = _labelKeysByStatus.TryGetValue(status, out string knownLabelKey);
        string labelKey = hasLabelKey ? knownLabelKey : OfflineLabelKey;
        return labelKey;
    }
}
