using System.Collections.Generic;

namespace DominoTrainGame.Client.ViewModel.Common;

public static class UsernameList
{
    private const string Separator = ", ";

    public static string Join(IEnumerable<string> usernames)
    {
        string joinedUsernames = string.Join(Separator, usernames);
        return joinedUsernames;
    }
}
