using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DominoTrainGame.Client.Models.Contracts;
using DominoTrainGame.Client.ViewModel.Commands;
using DominoTrainGame.Client.ViewModel.Common;
using DominoTrainGame.Client.ViewModel.Contracts;
using DominoTrainGame.Client.ViewModel.Localization;
using DominoTrainGame.Client.ViewModel.Validation;
using DominoTrainGame.Shared.Common;
using DominoTrainGame.Shared.Rules;

namespace DominoTrainGame.Client.ViewModel;

public sealed class LobbyChatViewModel : ObservableViewModel, ISubscribingViewModel
{
    private const int MaximumVisibleMessages = 100;
    private const string ChatMessageEmptyMessageKey = "ChatMessageEmptyMessage";
    private const string ChatMessageTooLongMessageKey = "ChatMessageTooLongMessage";

    private readonly IWindowService _windowService;
    private readonly IRemoteGameServices _gameServices;
    private readonly ILobbyEvents _lobbyEvents;
    private readonly LocalizationViewModel _localization;
    private string _messageText = string.Empty;
    private string _chatNotice = string.Empty;

    public LobbyChatViewModel(
        IWindowService windowService,
        IRemoteGameServices gameServices,
        LocalizationViewModel localization)
    {
        _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
        _gameServices = gameServices ?? throw new ArgumentNullException(nameof(gameServices));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _lobbyEvents = _gameServices.LobbyEvents;

        SendMessageCommand = new AsyncRelayCommand(SendMessageAsync);

        _lobbyEvents.LobbyMessageReceived += HandleLobbyMessageReceived;
    }

    public string MessageText
    {
        get
        {
            return _messageText;
        }
        set
        {
            SetProperty(ref _messageText, value);
        }
    }

    public string ChatNotice
    {
        get
        {
            return _chatNotice;
        }
        private set
        {
            bool hasChanged = SetProperty(ref _chatNotice, value);

            if (hasChanged)
            {
                OnPropertyChanged(nameof(HasChatNotice));
            }
        }
    }

    public bool HasChatNotice => !string.IsNullOrEmpty(ChatNotice);

    public int MaximumMessageLength => GameRules.MaximumMessageLength;

    public ObservableCollection<RoomChatMessageViewModel> Messages { get; } =
        new ObservableCollection<RoomChatMessageViewModel>();

    public AsyncRelayCommand SendMessageCommand { get; }

    public void Unsubscribe()
    {
        _lobbyEvents.LobbyMessageReceived -= HandleLobbyMessageReceived;
    }

    private void HandleLobbyMessageReceived(object sender, LobbyMessageReceivedEventArgs eventArgs)
    {
        RoomChatMessageViewModel message = new RoomChatMessageViewModel(eventArgs.Sender.Username, eventArgs.Text);

        Messages.Add(message);

        if (Messages.Count > MaximumVisibleMessages)
        {
            Messages.RemoveAt(CollectionIndex.First);
        }
    }

    private async Task SendMessageAsync()
    {
        LobbyMessageInputStatus inputStatus = LobbyMessageInputValidator.Validate(MessageText);

        if (inputStatus == LobbyMessageInputStatus.Valid)
        {
            await SubmitMessageAsync();
        }
        else
        {
            _windowService.ShowMessageError(GetInputMessageKey(inputStatus));
        }
    }

    private async Task SubmitMessageAsync()
    {
        LobbyActionResult sendResult = await RunWhileBusyAsync(_gameServices.SendLobbyMessageAsync, MessageText);
        bool isChatSanction = IsChatSanction(sendResult);
        bool wasDelivered = sendResult == LobbyActionResult.Success || sendResult == LobbyActionResult.ChatWarning;

        if (wasDelivered)
        {
            MessageText = string.Empty;
        }
        else if (!isChatSanction)
        {
            _windowService.ShowMessageError(LobbyResultMessageKeys.GetMessageKey(sendResult));
        }

        ChatNotice = isChatSanction
            ? _localization[LobbyResultMessageKeys.GetMessageKey(sendResult)]
            : string.Empty;
    }

    private static bool IsChatSanction(LobbyActionResult actionResult)
    {
        bool isChatSanction = actionResult == LobbyActionResult.ChatWarning
            || actionResult == LobbyActionResult.ChatBlocked;
        return isChatSanction;
    }

    private static string GetInputMessageKey(LobbyMessageInputStatus inputStatus)
    {
        string messageKey;

        switch (inputStatus)
        {
            case LobbyMessageInputStatus.MessageTooLong:
                messageKey = ChatMessageTooLongMessageKey;
                break;
            default:
                messageKey = ChatMessageEmptyMessageKey;
                break;
        }

        return messageKey;
    }
}
