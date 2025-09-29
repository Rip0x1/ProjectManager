using System.Windows;
using ProjectManagementSystem.WPF.Models;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class UserDetailsWindow : Window
    {
        public UserDetailsWindow(UserItem user)
        {
            InitializeComponent();
            DataContext = user;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

