using ProjectManagementSystem.WPF.Services;
using ProjectManagementSystem.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();

            var authService = App.GetService<IAuthService>();
            var navigationService = App.GetService<INavigationService>();
            var notificationService = App.GetService<INotificationService>();

            var viewModel = new LoginViewModel(authService, navigationService, notificationService);
            DataContext = viewModel;

            LoginSnackbar.MessageQueue = notificationService.MessageQueue;

            viewModel.PasswordChanged += (password) =>
            {
                PasswordBox.Password = password;
            };

            PasswordBox.PasswordChanged += (s, e) =>
            {
                viewModel.Password = PasswordBox.Password;
            };
        }
    }
}