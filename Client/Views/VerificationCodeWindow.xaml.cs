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

    private void OnDigitTextBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = true;

        int index = GetFocusedIndex();

        bool hasFocusedTextBox = index >= FirstDigitTextBoxIndex;
        bool canAcceptInput = hasFocusedTextBox && e.Text.Length == SingleDigitInputLength;

        if (canAcceptInput && IsAsciiDigit(e.Text[FirstCharacterIndex]))
        {
            SetDigit(index, e.Text);
            FocusDigitTextBox(index + DigitTextBoxNavigationStep);
        }
    }

    private void OnDigitTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
    {
        int index = GetFocusedIndex();

        if (index >= FirstDigitTextBoxIndex)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
            else if (e.Key == Key.Back)
            {
                e.Handled = true;
                DeleteBackward(index);
            }
            else if (e.Key == Key.Delete)
            {
                e.Handled = true;
                SetDigit(index, string.Empty);
            }
            else if (e.Key == Key.Left)
            {
                e.Handled = true;
                FocusDigitTextBox(index - DigitTextBoxNavigationStep);
            }
            else if (e.Key == Key.Right)
            {
                e.Handled = true;
                FocusDigitTextBox(index + DigitTextBoxNavigationStep);
            }
        }
    }

    private void OnDigitTextBoxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        (e.NewFocus as TextBox)?.SelectAll();
    }

    private void OnDigitTextBoxPasting(object sender, DataObjectPastingEventArgs e)
    {
        e.CancelCommand();

        int index = GetFocusedIndex();
        string pastedText = e.DataObject.GetData(DataFormats.UnicodeText) as string ?? string.Empty;
        bool hasFocusedTextBox = index >= FirstDigitTextBoxIndex;
        bool hasPastedText = pastedText.Length > EmptyTextLength;

        if (hasFocusedTextBox && hasPastedText)
        {
            PasteDigits(index, pastedText);
        }
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
