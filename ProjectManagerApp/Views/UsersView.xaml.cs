using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;
using ProjectManagementSystem.WPF.ViewModels;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class UsersView : UserControl
    {
        public UsersView()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                var usersService = App.GetService<IUsersService>();
                var notificationService = App.GetService<INotificationService>();

                var viewModel = new UsersViewModel(usersService, notificationService);
                DataContext = viewModel;

                viewModel.LoadAsync();
            }
        }

        private void UserCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is UserItem user)
            {
                var detailsWindow = new UserDetailsWindow(user);
                detailsWindow.Owner = Window.GetWindow(this);
                detailsWindow.ShowDialog();
            }
        }
    }
}
