using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DominoTrainGame.Shared.Common;
using DominoTrainGame.Shared.Rules;

namespace DominoTrainGame.Client.View.Views;

public sealed partial class VerificationCodeWindow : Window
{
    private const int SingleDigit = 1;
    private const int LastDigitTextBoxIndex = GameRules.VerificationCodeLength - SingleDigit;
    private const char FirstAsciiDigit = '0';
    private const char LastAsciiDigit = '9';

    private readonly TextBox[] _digitTextBoxes;

    public VerificationCodeWindow()
    {
        InitializeComponent();
        _digitTextBoxes = new[]
        {
            FirstDigitTextBox,
            SecondDigitTextBox,
            ThirdDigitTextBox,
            FourthDigitTextBox,
            FifthDigitTextBox,
            SixthDigitTextBox
        };
    }

    private void OnDigitTextBoxPreviewTextInput(object sender, TextCompositionEventArgs eventArgs)
    {
        eventArgs.Handled = true;

        int index = GetFocusedIndex();

        bool hasFocusedTextBox = index >= CollectionIndex.First;
        bool canAcceptInput = hasFocusedTextBox && eventArgs.Text.Length == SingleDigit;

        if (canAcceptInput && IsAsciiDigit(eventArgs.Text[CollectionIndex.First]))
        {
            SetDigit(index, eventArgs.Text);
            FocusDigitTextBox(index + SingleDigit);
        }
    }

    private void OnDigitTextBoxPreviewKeyDown(object sender, KeyEventArgs eventArgs)
    {
        int index = GetFocusedIndex();
        bool hasFocusedTextBox = index >= CollectionIndex.First;
        bool isHandled = hasFocusedTextBox && HandleDigitKey(eventArgs.Key, index);

        if (isHandled)
        {
            eventArgs.Handled = true;
        }
    }

    private void OnDigitTextBoxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs eventArgs)
    {
        (eventArgs.NewFocus as TextBox)?.SelectAll();
    }

    private void OnDigitTextBoxPasting(object sender, DataObjectPastingEventArgs eventArgs)
    {
        eventArgs.CancelCommand();

        int index = GetFocusedIndex();
        string pastedText = eventArgs.DataObject.GetData(DataFormats.UnicodeText) as string ?? string.Empty;
        bool hasFocusedTextBox = index >= CollectionIndex.First;
        bool hasPastedText = pastedText.Length > InputText.EmptyLength;

        if (hasFocusedTextBox && hasPastedText)
        {
            PasteDigits(index, pastedText);
        }
    }

    private bool HandleDigitKey(Key key, int index)
    {
        bool isHandled = true;

        switch (key)
        {
            case Key.Space:
                break;
            case Key.Back:
                DeleteBackward(index);
                break;
            case Key.Delete:
                SetDigit(index, string.Empty);
                break;
            case Key.Left:
                FocusDigitTextBox(index - SingleDigit);
                break;
            case Key.Right:
                FocusDigitTextBox(index + SingleDigit);
                break;
            default:
                isHandled = false;
                break;
        }

        return isHandled;
    }

    private void PasteDigits(int focusedIndex, string pastedText)
    {
        string digits = new string(pastedText.Where(IsAsciiDigit).ToArray());
        int firstIndex = digits.Length >= GameRules.VerificationCodeLength ? CollectionIndex.First : focusedIndex;

        for (int offset = CollectionIndex.First;
            offset < digits.Length && firstIndex + offset < GameRules.VerificationCodeLength;
            offset++)
        {
            SetDigit(firstIndex + offset, digits[offset].ToString());
        }

        FocusDigitTextBox(Math.Min(firstIndex + digits.Length, LastDigitTextBoxIndex));
    }

    private int GetFocusedIndex()
    {
        int focusedIndex = Array.IndexOf(_digitTextBoxes, Keyboard.FocusedElement as TextBox);
        return focusedIndex;
    }

    private void FocusDigitTextBox(int index)
    {
        bool isInsideRange = index >= CollectionIndex.First && index <= LastDigitTextBoxIndex;

        if (isInsideRange)
        {
            _digitTextBoxes[index].Focus();
        }
    }

    private void DeleteBackward(int index)
    {
        if (!string.IsNullOrEmpty(_digitTextBoxes[index].Text))
        {
            SetDigit(index, string.Empty);
        }
        else if (index > CollectionIndex.First)
        {
            SetDigit(index - SingleDigit, string.Empty);
            FocusDigitTextBox(index - SingleDigit);
        }
    }

    private void SetDigit(int index, string text)
    {
        _digitTextBoxes[index].SetCurrentValue(TextBox.TextProperty, text);
    }

    private static bool IsAsciiDigit(char character)
    {
        bool isAscii = character >= FirstAsciiDigit && character <= LastAsciiDigit;
        return isAscii;
    }
}
