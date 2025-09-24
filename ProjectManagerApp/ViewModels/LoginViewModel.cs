using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManagementSystem.WPF.Services;
using ProjectManagementSystem.WPF.Views;
using ProjectManagerApp;
using System.Threading.Tasks;
using System.Windows;

namespace ProjectManagementSystem.WPF.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private string _email = "";

        [ObservableProperty]
        private string _password = "";

        [ObservableProperty]
        private string _errorMessage = "";

        [ObservableProperty]
        private bool _isLoading = false;

        public event System.Action<string>? PasswordChanged;

        public LoginViewModel(IAuthService authService, INavigationService navigationService, INotificationService notificationService)
        {
            _authService = authService;
            _navigationService = navigationService;
            _notificationService = notificationService;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                _notificationService.ShowWarning("Пожалуйста, введите email и пароль");
                return;
            }

            IsLoading = true;
            ErrorMessage = "";

            try
            {
                var result = await _authService.LoginAsync(Email, Password);

                if (result != null)
                {
                    _notificationService.ShowSuccess($"Добро пожаловать, {result.FirstName}!");

                    await Task.Delay(1000);
                    _navigationService.NavigateToDashboard();
                }
                else
                {
                    _notificationService.ShowError("Неверный email или пароль");
                }
            }
            catch (System.Exception ex)
            {
                _notificationService.ShowError($"Ошибка входа: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void Exit()
        {
            Application.Current.Shutdown();
        }

        [RelayCommand]
        private void UseDemoAdmin()
        {
            Email = "admin@test.com";
            Password = "admin123";
            PasswordChanged?.Invoke(Password);
            _notificationService.ShowInfo("Демо-данные загружены. Нажмите 'Войти'");
        }

        partial void OnPasswordChanged(string value)
        {
            PasswordChanged?.Invoke(value);
        }
    }
}