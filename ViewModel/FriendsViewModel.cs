using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Common;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Localization;
using DominoTrainGame.Client.ViewModel.Validation;

namespace DominoTrainGame.Client.ViewModel;

public sealed class FriendsViewModel : ScreenViewModel, ISubscribingViewModel
{
    private const string FriendRequestSentMessageKey = "FriendRequestSentMessage";
    private const string FriendRequestAcceptedMessageKey = "FriendRequestAcceptedMessage";
    private const string FriendRequestRejectedMessageKey = "FriendRequestRejectedMessage";
    private const string FriendRemovedMessageKey = "FriendRemovedMessage";
    private const int SearchDelayMilliseconds = 300;
    private const int MinimumSearchLength = 2;
    private const int EmptyCount = 0;

    private static readonly TimeSpan _searchDelay;

    private readonly IRemoteSocialServices _socialServices;
    private readonly LocalizationViewModel _localization;
    private bool _isFriendsSelected;
    private bool _isRequestsSelected;
    private string _friendUsername;
    private CancellationTokenSource _searchCancellation;

    static FriendsViewModel()
    {
        _searchDelay = TimeSpan.FromMilliseconds(SearchDelayMilliseconds);
    }

    public FriendsViewModel(
        IWindowService windowService,
        IRemoteSocialServices socialServices,
        LocalizationViewModel localization) : base(windowService)
    {
        _socialServices = socialServices ?? throw new ArgumentNullException(nameof(socialServices));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _isFriendsSelected = true;
        _friendUsername = string.Empty;
        _searchCancellation = new CancellationTokenSource();

        AddFriendCommand = new AsyncRelayCommand(SendFriendRequestAsync);
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        SearchCommand = new AsyncRelayCommand(SearchPlayersAsync);
        _socialServices.FriendStatusChanged += HandleFriendStatusChanged;
        _socialServices.FriendsChanged += HandleFriendsChanged;
        RefreshCommand.Execute(null);
    }

    public bool IsFriendsSelected
    {
        get
        {
            return _isFriendsSelected;
        }
        set
        {
            bool hasChanged = SetProperty(ref _isFriendsSelected, value);

            if (hasChanged && value)
            {
                RefreshCommand.Execute(null);
            }
        }
    }

    public bool IsRequestsSelected
    {
        get
        {
            return _isRequestsSelected;
        }
        set
        {
            bool hasChanged = SetProperty(ref _isRequestsSelected, value);

            if (hasChanged && value)
            {
                RefreshCommand.Execute(null);
            }
        }
    }

    public string FriendUsername
    {
        get
        {
            return _friendUsername;
        }
        set
        {
            bool hasChanged = SetProperty(ref _friendUsername, value ?? string.Empty);

            if (hasChanged)
            {
                RestartSearch();
            }
        }
    }

    public ObservableCollection<FriendItemViewModel> Friends { get; } =
        new ObservableCollection<FriendItemViewModel>();

    public ObservableCollection<FriendRequestItemViewModel> PendingRequests { get; } =
        new ObservableCollection<FriendRequestItemViewModel>();

    public ObservableCollection<PlayerSearchItemViewModel> SearchResults { get; } =
        new ObservableCollection<PlayerSearchItemViewModel>();

    public bool HasSearchResults => SearchResults.Count > EmptyCount;

    public AsyncRelayCommand AddFriendCommand { get; }

    public AsyncRelayCommand RefreshCommand { get; }

    public AsyncRelayCommand SearchCommand { get; }

    public void Unsubscribe()
    {
        _socialServices.FriendStatusChanged -= HandleFriendStatusChanged;
        _socialServices.FriendsChanged -= HandleFriendsChanged;
        _searchCancellation.Cancel();
    }

    private async Task RefreshAsync()
    {
        FriendListResult friendsResult = await RunWhileBusyAsync(() => _socialServices.GetFriendsAsync());
        bool isFriendListLoaded = friendsResult.Status == FriendResultStatus.Success;

        if (isFriendListLoaded)
        {
            ShowFriends(friendsResult.Friends);
            FriendRequestListResult requestsResult = await RunWhileBusyAsync(
                () => _socialServices.GetPendingRequestsAsync());
            ShowPendingRequests(requestsResult);
        }
        else
        {
            ShowFailure(friendsResult.Status);
        }
    }

    private void ShowFriends(IReadOnlyList<FriendResult> friends)
    {
        Friends.Clear();

        foreach (FriendResult friend in friends)
        {
            Friends.Add(CreateFriendItem(friend));
        }
    }

    private FriendItemViewModel CreateFriendItem(FriendResult friend)
    {
        string statusLabel = _localization[FriendStatusLabelKeys.GetLabelKey(friend.Status)];
        FriendItemViewModel friendItem = new FriendItemViewModel(friend, statusLabel, RemoveFriendAsync);
        return friendItem;
    }

    private void HandleFriendStatusChanged(object sender, FriendStatusChangedEventArgs eventArgs)
    {
        FriendItemViewModel friendItem = Friends.FirstOrDefault(item => item.PlayerId == eventArgs.PlayerId);

        if (friendItem != null)
        {
            FriendResult friend = new FriendResult(eventArgs.PlayerId, friendItem.Username, eventArgs.Status);
            Friends[Friends.IndexOf(friendItem)] = CreateFriendItem(friend);
        }
    }

    private void HandleFriendsChanged(object sender, EventArgs eventArgs)
    {
        RefreshCommand.Execute(null);
    }

    private void ShowPendingRequests(FriendRequestListResult requestsResult)
    {
        if (requestsResult.Status == FriendResultStatus.Success)
        {
            PendingRequests.Clear();

            foreach (FriendRequestResult request in requestsResult.Requests)
            {
                PendingRequests.Add(new FriendRequestItemViewModel(request, AcceptRequestAsync, RejectRequestAsync));
            }
        }
        else
        {
            ShowFailure(requestsResult.Status);
        }
    }

    private async Task SendFriendRequestAsync()
    {
        FriendUsernameInputStatus inputStatus = FriendUsernameInputValidator.Validate(FriendUsername);

        if (inputStatus == FriendUsernameInputStatus.Valid)
        {
            string username = FriendUsernameInputValidator.Normalize(FriendUsername);
            FriendResultStatus status = await RunWhileBusyAsync(() => _socialServices.SendFriendRequestAsync(username));
            ShowSendResult(status);
        }
        else
        {
            WindowService.ShowMessage(_localization[FriendUsernameInputMessageKeys.GetMessageKey(inputStatus)]);
        }
    }

    private void RestartSearch()
    {
        _searchCancellation.Cancel();
        _searchCancellation.Dispose();
        _searchCancellation = new CancellationTokenSource();
        SearchCommand.Execute(null);
    }

    private async Task SearchPlayersAsync()
    {
        CancellationToken cancellationToken = _searchCancellation.Token;
        string usernameFragment = FriendUsernameInputValidator.Normalize(FriendUsername);
        bool isLongEnough = usernameFragment.Length >= MinimumSearchLength;
        bool isValid = FriendUsernameInputValidator.Validate(usernameFragment) == FriendUsernameInputStatus.Valid;

        if (isLongEnough && isValid)
        {
            await Task.Delay(_searchDelay, cancellationToken);
            PlayerSearchResult searchResult = await _socialServices.SearchPlayersAsync(usernameFragment);
            ShowSearchResults(usernameFragment, searchResult);
        }
        else
        {
            ClearSearchResults();
        }
    }

    private void ShowSearchResults(string usernameFragment, PlayerSearchResult searchResult)
    {
        string currentFragment = FriendUsernameInputValidator.Normalize(FriendUsername);
        bool isCurrentSearch = string.Equals(usernameFragment, currentFragment, StringComparison.Ordinal);

        if (isCurrentSearch)
        {
            SearchResults.Clear();

            foreach (FoundPlayerResult player in searchResult.Players)
            {
                SearchResults.Add(new PlayerSearchItemViewModel(player, SendRequestToPlayerAsync));
            }

            OnPropertyChanged(nameof(HasSearchResults));
        }
    }

    private void ClearSearchResults()
    {
        SearchResults.Clear();
        OnPropertyChanged(nameof(HasSearchResults));
    }

    private async Task SendRequestToPlayerAsync(FoundPlayerResult player)
    {
        FriendResultStatus status = await RunWhileBusyAsync(
            () => _socialServices.SendFriendRequestAsync(player.Username));
        ShowSendResult(status);
    }

    private void ShowSendResult(FriendResultStatus status)
    {
        if (status == FriendResultStatus.Success)
        {
            FriendUsername = string.Empty;
            WindowService.ShowMessage(_localization[FriendRequestSentMessageKey]);
        }
        else
        {
            ShowFailure(status);
        }
    }

    private async Task RemoveFriendAsync(FriendResult friend)
    {
        FriendResultStatus status = await RunWhileBusyAsync(() => _socialServices.RemoveFriendAsync(friend.Username));
        await ApplyCommandResultAsync(status, FriendRemovedMessageKey);
    }

    private async Task AcceptRequestAsync(FriendRequestResult request)
    {
        FriendResultStatus status = await RunWhileBusyAsync(
            () => _socialServices.AcceptFriendRequestAsync(request.RequestId));
        await ApplyCommandResultAsync(status, FriendRequestAcceptedMessageKey);
    }

    private async Task RejectRequestAsync(FriendRequestResult request)
    {
        FriendResultStatus status = await RunWhileBusyAsync(
            () => _socialServices.RejectFriendRequestAsync(request.RequestId));
        await ApplyCommandResultAsync(status, FriendRequestRejectedMessageKey);
    }

    private async Task ApplyCommandResultAsync(FriendResultStatus status, string successMessageKey)
    {
        if (status == FriendResultStatus.Success)
        {
            WindowService.ShowMessage(_localization[successMessageKey]);
        }
        else
        {
            ShowFailure(status);
        }

        await RefreshAsync();
    }

    private void ShowFailure(FriendResultStatus status)
    {
        WindowService.ShowMessage(_localization[FriendResultMessageKeys.GetMessageKey(status)]);
    }
}
