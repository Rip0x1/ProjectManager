using ProjectManagementSystem.WPF.Services;
using ProjectManagementSystem.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();

            var authService = App.GetService<IAuthService>();
            var navigationService = App.GetService<INavigationService>();
            var loginNotificationService = App.GetService<ILoginNotificationService>();

            var viewModel = new LoginViewModel(authService, navigationService, loginNotificationService);
            DataContext = viewModel;

            LoginSnackbar.MessageQueue = loginNotificationService.MessageQueue;
            
            var notificationBorder = this.FindName("NotificationBorder") as System.Windows.Controls.Border;
            if (notificationBorder != null)
            {
                loginNotificationService.SetNotificationBorder(notificationBorder);
            }

            PasswordBox.PasswordChanged += (s, e) =>
            {
                if (PasswordBox.Password != viewModel.Password)
                {
                    viewModel.Password = PasswordBox.Password;
                }
            };

            if (RegisterPasswordBox != null)
            {
                RegisterPasswordBox.PasswordChanged += (s, e) =>
                {
                    if (RegisterPasswordBox.Password != viewModel.RegisterPassword)
                    {
                        viewModel.RegisterPassword = RegisterPasswordBox.Password;
                    }
                };
            }
        }
    }
}