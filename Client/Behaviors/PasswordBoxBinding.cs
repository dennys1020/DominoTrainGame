using System.Windows;
using System.Windows.Controls;

namespace DominoTrainGame.Behaviors;

public static class PasswordBoxBinding
{
    private const string BoundPasswordPropertyName = "BoundPassword";

    public static readonly DependencyProperty BoundPasswordProperty;

    static PasswordBoxBinding()
    {
        FrameworkPropertyMetadata metadata = new FrameworkPropertyMetadata(
            null,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnBoundPasswordChanged);

        BoundPasswordProperty = DependencyProperty.RegisterAttached(
            BoundPasswordPropertyName,
            typeof(string),
            typeof(PasswordBoxBinding),
            metadata);
    }

    public static string GetBoundPassword(DependencyObject target)
    {
        string boundPassword = (string)target.GetValue(BoundPasswordProperty);
        return boundPassword;
    }

    public static void SetBoundPassword(DependencyObject target, string value)
    {
        target.SetValue(BoundPasswordProperty, value);
    }

    private static void OnBoundPasswordChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
    {
        if (target is PasswordBox passwordBox)
        {
            string newPassword = (string)e.NewValue ?? string.Empty;

            passwordBox.PasswordChanged -= OnPasswordChanged;

            if (passwordBox.Password != newPassword)
            {
                passwordBox.Password = newPassword;
            }

            passwordBox.PasswordChanged += OnPasswordChanged;
        }
    }

    private static void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        PasswordBox passwordBox = (PasswordBox)sender;
        SetBoundPassword(passwordBox, passwordBox.Password);
    }
}
