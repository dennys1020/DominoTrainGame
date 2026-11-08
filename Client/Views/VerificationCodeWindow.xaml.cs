using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DominoTrainGame.Client.View.Views;

public sealed partial class VerificationCodeWindow : Window
{
    private const int CodeLength = 6;
    private const int SingleDigitInputLength = 1;
    private const int DigitTextBoxNavigationStep = 1;
    private const int FirstDigitTextBoxIndex = 0;
    private const int LastDigitTextBoxIndex = CodeLength - 1;
    private const int FirstCharacterIndex = 0;
    private const int FirstPastedDigitOffset = 0;
    private const int EmptyTextLength = 0;
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

        bool hasFocusedTextBox = index >= FirstDigitTextBoxIndex;
        bool canAcceptInput = hasFocusedTextBox && eventArgs.Text.Length == SingleDigitInputLength;

        if (canAcceptInput && IsAsciiDigit(eventArgs.Text[FirstCharacterIndex]))
        {
            SetDigit(index, eventArgs.Text);
            FocusDigitTextBox(index + DigitTextBoxNavigationStep);
        }
    }

    private void OnDigitTextBoxPreviewKeyDown(object sender, KeyEventArgs eventArgs)
    {
        int index = GetFocusedIndex();
        bool hasFocusedTextBox = index >= FirstDigitTextBoxIndex;
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
        bool hasFocusedTextBox = index >= FirstDigitTextBoxIndex;
        bool hasPastedText = pastedText.Length > EmptyTextLength;

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
                FocusDigitTextBox(index - DigitTextBoxNavigationStep);
                break;
            case Key.Right:
                FocusDigitTextBox(index + DigitTextBoxNavigationStep);
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
        int firstIndex = digits.Length >= CodeLength ? FirstDigitTextBoxIndex : focusedIndex;

        for (int offset = FirstPastedDigitOffset; offset < digits.Length && firstIndex + offset < CodeLength; offset++)
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
        bool isInsideRange = index >= FirstDigitTextBoxIndex && index <= LastDigitTextBoxIndex;

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
        else if (index > FirstDigitTextBoxIndex)
        {
            SetDigit(index - DigitTextBoxNavigationStep, string.Empty);
            FocusDigitTextBox(index - DigitTextBoxNavigationStep);
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
