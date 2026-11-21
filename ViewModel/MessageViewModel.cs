using System;
using System.Collections.Generic;
using DominoTrainGame.Client.ViewModel.Localization;
using DominoTrainGame.Client.ViewModel.Navigation;

namespace DominoTrainGame.Client.ViewModel;

public sealed class MessageViewModel
{
    private const string InformationHeadingKey = "InformationMessageTitle";
    private const string ErrorHeadingKey = "ErrorMessageTitle";
    private const string ConfirmationHeadingKey = "ConfirmationMessageTitle";

    private static readonly IReadOnlyDictionary<MessageKind, string> _headingKeysByKind;

    static MessageViewModel()
    {
        _headingKeysByKind = new Dictionary<MessageKind, string>
        {
            [MessageKind.Information] = InformationHeadingKey,
            [MessageKind.Error] = ErrorHeadingKey,
            [MessageKind.Confirmation] = ConfirmationHeadingKey
        };
    }

    public MessageViewModel(string message, MessageKind kind, LocalizationViewModel localization)
    {
        if (localization == null)
        {
            throw new ArgumentNullException(nameof(localization));
        }

        if (!_headingKeysByKind.ContainsKey(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }

        Message = message ?? throw new ArgumentNullException(nameof(message));
        Heading = localization[_headingKeysByKind[kind]];
        IsError = kind == MessageKind.Error;
        IsConfirmation = kind == MessageKind.Confirmation;
    }

    public string Heading { get; }

    public string Message { get; }

    public bool IsError { get; }

    public bool IsConfirmation { get; }

    public bool IsAcknowledgement => !IsConfirmation;
}
