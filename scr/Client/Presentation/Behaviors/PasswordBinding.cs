using System.Windows;
using System.Windows.Controls;

namespace DominoTrainGame.Behaviors;

// PasswordBox.Password is not a dependency property. This view behavior bridges the existing input to the ViewModel.
public static class PasswordBinding
{
    public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
        "IsEnabled", typeof(bool), typeof(PasswordBinding), new PropertyMetadata(false, OnBindingChanged));
    public static readonly DependencyProperty PasswordProperty = DependencyProperty.RegisterAttached(
        "Password", typeof(string), typeof(PasswordBinding),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordChanged));
    public static string GetPassword(DependencyObject target) => (string)target.GetValue(PasswordProperty);
    public static void SetPassword(DependencyObject target, string value) => target.SetValue(PasswordProperty, value);
    public static bool GetIsEnabled(DependencyObject target) => (bool)target.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject target, bool value) => target.SetValue(IsEnabledProperty, value);

    private static void OnBindingChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
    {
        if (target is PasswordBox box)
        {
            box.PasswordChanged -= OnInputChanged;
            if ((bool)args.NewValue) { box.PasswordChanged += OnInputChanged; }
        }
    }

    private static void OnPasswordChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
    {
        if (target is PasswordBox box)
        {
            box.PasswordChanged -= OnInputChanged;
            if (box.Password != (string)args.NewValue) { box.Password = (string)args.NewValue ?? string.Empty; }
            if (GetIsEnabled(box)) { box.PasswordChanged += OnInputChanged; }
        }
    }

    private static void OnInputChanged(object sender, RoutedEventArgs args)
    {
        PasswordBox box = (PasswordBox)sender;
        box.SetCurrentValue(PasswordProperty, box.Password);
    }
}
