using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DominoTrainGame.Views;

public partial class VerificationCodeWindow : Window
{
    private const int CodeLength = 6;
    private const int SingleDigitInputLength = 1;
    private const int DigitBoxNavigationStep = 1;
    private const int FirstDigitBoxIndex = 0;
    private const char FirstAsciiDigit = '0';
    private const char LastAsciiDigit = '9';
    private readonly TextBox[] _digitBoxes;

    public VerificationCodeWindow()
    {
        InitializeComponent();
        _digitBoxes = new[] { digitBox1, digitBox2, digitBox3, digitBox4, digitBox5, digitBox6 };
    }

    private void OnDigitBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = true;

        int index = GetFocusedIndex();

        bool hasFocusedBox = index >= FirstDigitBoxIndex;
        bool canAcceptInput = hasFocusedBox && e.Text.Length == SingleDigitInputLength;

        if (canAcceptInput && IsAsciiDigit(e.Text[FirstDigitBoxIndex]))
        {
            SetDigit(index, e.Text);
            FocusDigitBox(index + DigitBoxNavigationStep);
        }
    }

    private void OnDigitBoxPreviewKeyDown(object sender, KeyEventArgs e)
    {
        int index = GetFocusedIndex();

        if (index >= FirstDigitBoxIndex)
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
                FocusDigitBox(index - DigitBoxNavigationStep);
            }
            else if (e.Key == Key.Right)
            {
                e.Handled = true;
                FocusDigitBox(index + DigitBoxNavigationStep);
            }
        }
    }

    private void OnDigitBoxGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        TextBox focusedBox = e.NewFocus as TextBox;

        if (focusedBox != null)
        {
            focusedBox.SelectAll();
        }
    }

    private void OnDigitBoxPasting(object sender, DataObjectPastingEventArgs e)
    {
        e.CancelCommand();

        int index = GetFocusedIndex();
        string pastedText = e.DataObject.GetData(DataFormats.UnicodeText) as string;

        if (index >= FirstDigitBoxIndex && pastedText != null)
        {
            string digits = new string(pastedText.Where(IsAsciiDigit).ToArray());
            int firstIndex = digits.Length >= CodeLength ? FirstDigitBoxIndex : index;

            for (int offset = 0; offset < digits.Length && firstIndex + offset < CodeLength; offset++)
            {
                SetDigit(firstIndex + offset, digits[offset].ToString());
            }

            FocusDigitBox(Math.Min(firstIndex + digits.Length, CodeLength - DigitBoxNavigationStep));
        }
    }

    private int GetFocusedIndex()
    {
        int focusedIndex;
        focusedIndex = Array.IndexOf(_digitBoxes, Keyboard.FocusedElement as TextBox);
        return focusedIndex;
    }

    private void FocusDigitBox(int index)
    {
        if (index >= FirstDigitBoxIndex && index < CodeLength)
        {
            _digitBoxes[index].Focus();
        }
    }

    private void FocusFirstEmptyBox()
    {
        TextBox firstEmptyBox = _digitBoxes.FirstOrDefault(digitBox => string.IsNullOrEmpty(digitBox.Text));

        if (firstEmptyBox != null)
        {
            firstEmptyBox.Focus();
        }
    }

    private void DeleteBackward(int index)
    {
        if (!string.IsNullOrEmpty(_digitBoxes[index].Text))
        {
            SetDigit(index, string.Empty);
        }
        else if (index > FirstDigitBoxIndex)
        {
            SetDigit(index - DigitBoxNavigationStep, string.Empty);
            FocusDigitBox(index - DigitBoxNavigationStep);
        }
    }

    private void SetDigit(int index, string text)
    {
        _digitBoxes[index].SetCurrentValue(TextBox.TextProperty, text);
    }

    private static bool IsAsciiDigit(char character)
    {
        bool isAscii;
        isAscii = character >= FirstAsciiDigit && character <= LastAsciiDigit;
        return isAscii;
    }
}
