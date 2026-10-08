using System.Windows;
using System.Windows.Controls;

namespace ProjectManagementSystem.WPF.Helpers
{
    public static class PasswordBoxInputRestriction
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(PasswordBoxInputRestriction),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject obj) =>
            (bool)obj.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(DependencyObject obj, bool value) =>
            obj.SetValue(IsEnabledProperty, value);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not PasswordBox passwordBox)
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                passwordBox.MaxLength = ValidationLimits.PasswordMax;
                passwordBox.PasswordChanged += OnPasswordChanged;
            }
            else
            {
                passwordBox.PasswordChanged -= OnPasswordChanged;
            }
        }

        private static void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is not PasswordBox passwordBox)
            {
                return;
            }

            var sanitized = InputValidation.SanitizePassword(passwordBox.Password);
            if (sanitized == passwordBox.Password)
            {
                return;
            }

            var caret = passwordBox.Password.Length;
            passwordBox.Password = sanitized;
        }
    }
}
