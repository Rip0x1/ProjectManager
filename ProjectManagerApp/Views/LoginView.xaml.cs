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
            
            // Находим Border для уведомлений и устанавливаем его в сервис
            var notificationBorder = this.FindName("NotificationBorder") as System.Windows.Controls.Border;
            if (notificationBorder != null)
            {
                loginNotificationService.SetNotificationBorder(notificationBorder);
            }

            PasswordBox.PasswordChanged += (s, e) =>
            {
                viewModel.Password = PasswordBox.Password;
            };

            if (RegisterPasswordBox != null)
            {
                RegisterPasswordBox.PasswordChanged += (s, e) =>
                {
                    viewModel.RegisterPassword = RegisterPasswordBox.Password;
                };
            }

            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(viewModel.Password))
                {
                    PasswordBox.Password = viewModel.Password;
                }
            };
        }
    }
}