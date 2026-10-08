using System.Windows;
using System.Windows.Input;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class UserRelatedListWindow : Window
    {
        public UserRelatedListWindow()
        {
            InitializeComponent();
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
