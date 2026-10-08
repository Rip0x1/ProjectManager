using System.Windows;
using System.Windows.Input;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class ConfirmClearLogsWindow : Window
    {
        public ConfirmClearLogsWindow(string summary, int count)
        {
            InitializeComponent();
            DataContext = new ConfirmClearLogsModel(summary, count);
        }

        public static bool Confirm(Window? owner, string summary, int count)
        {
            var window = new ConfirmClearLogsWindow(summary, count);
            if (owner != null)
            {
                window.Owner = owner;
            }

            return window.ShowDialog() == true;
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private sealed class ConfirmClearLogsModel
        {
            public ConfirmClearLogsModel(string summary, int count)
            {
                Summary = summary;
                CountText = count.ToString("N0");
            }

            public string Summary { get; }
            public string CountText { get; }
        }
    }
}
