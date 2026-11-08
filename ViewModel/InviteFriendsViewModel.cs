using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Common;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Localization;

namespace DominoTrainGame.Client.ViewModel;

public sealed class InviteFriendsViewModel : ObservableViewModel
{
    private readonly IWindowService _windowService;
    private readonly IRemoteGameServices _gameServices;
    private readonly IRemoteSocialServices _socialServices;
    private bool _isOpen;

    public InviteFriendsViewModel(
        IWindowService windowService,
        IRemoteGameServices gameServices,
        IRemoteSocialServices socialServices)
    {
        _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
        _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        _socialServices = socialServices ?? throw new ArgumentNullException(nameof(socialServices));

        OpenCommand = new AsyncRelayCommand(OpenAsync);
    }

    public bool IsOpen
    {
        get
        {
            return _isOpen;
        }
        set
        {
            SetProperty(ref _isOpen, value);
        }
    }

    public ObservableCollection<InvitableFriendViewModel> Friends { get; } =
        new ObservableCollection<InvitableFriendViewModel>();

    public AsyncRelayCommand OpenCommand { get; }

    private async Task OpenAsync()
    {
        FriendListResult friendsResult = await RunWhileBusyAsync(() => _socialServices.GetFriendsAsync());

        if (friendsResult.Status == FriendResultStatus.Success)
        {
            ShowFriends(friendsResult.Friends);
            IsOpen = true;
        }
        else
        {
            _windowService.ShowMessage(FriendResultMessageKeys.GetMessageKey(friendsResult.Status));
        }
    }

    private void ShowFriends(IReadOnlyList<FriendResult> friends)
    {
        Friends.Clear();

        foreach (FriendResult friend in friends)
        {
            Friends.Add(new InvitableFriendViewModel(friend, InviteAsync));
        }
    }

    private async Task InviteAsync(InvitableFriendViewModel friend)
    {
        LobbyResultStatus result = await RunWhileBusyAsync(() => _gameServices.InviteFriendAsync(friend.Username));

        if (result == LobbyResultStatus.Success)
        {
            friend.MarkInvited();
        }
        else
        {
            IsOpen = false;
            _windowService.ShowMessage(LobbyResultMessageKeys.GetMessageKey(result));
        }
    }
}
