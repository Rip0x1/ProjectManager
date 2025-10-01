using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManagementSystem.WPF.Services;
using System.Windows;
using System.Windows.Media;

namespace ProjectManagementSystem.WPF.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private readonly ILoginNotificationService _notificationService;

        [ObservableProperty]
        private string _email = "";

        [ObservableProperty]
        private string _password = "";

        [ObservableProperty]
        private string _errorMessage = "";

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private bool _isEmailInvalid = false;

        [ObservableProperty]
        private bool _isPasswordInvalid = false;

        [ObservableProperty]
        private bool _isFirstNameInvalid = false;

        [ObservableProperty]
        private bool _isLastNameInvalid = false;

        [ObservableProperty]
        private bool _isRegisterEmailInvalid = false;

        [ObservableProperty]
        private bool _isRegisterPasswordInvalid = false;

        [ObservableProperty]
        private bool _isRegisterMode = false;

        [ObservableProperty]
        private bool _isLoginMode = true;

        [ObservableProperty]
        private string _firstName = "";

        [ObservableProperty]
        private string _lastName = "";

        [ObservableProperty]
        private string _registerEmail = "";

        [ObservableProperty]
        private string _registerPassword = "";

        [ObservableProperty]
        private Brush _snackbarBackground = new SolidColorBrush(Colors.Transparent);

        [ObservableProperty]
        private bool _isPasswordVisible = false;

        [ObservableProperty]
        private bool _isRegisterPasswordVisible = false;

        public event System.Action<string>? PasswordChanged;

        public LoginViewModel(IAuthService authService, INavigationService navigationService, ILoginNotificationService notificationService)
        {
            _authService = authService;
            _navigationService = navigationService;
            _notificationService = notificationService;
        }

        [RelayCommand]
        private void ShowRegister()
        {
            IsRegisterMode = true;
            IsLoginMode = false;
            ErrorMessage = "";
            IsEmailInvalid = false;
            IsPasswordInvalid = false;
            IsFirstNameInvalid = false;
            IsLastNameInvalid = false;
            IsRegisterEmailInvalid = false;
            IsRegisterPasswordInvalid = false;
        }

        [RelayCommand]
        private void ShowLogin()
        {
            IsRegisterMode = false;
            IsLoginMode = true;
            ErrorMessage = "";
            IsEmailInvalid = false;
            IsPasswordInvalid = false;
            IsFirstNameInvalid = false;
            IsLastNameInvalid = false;
            IsRegisterEmailInvalid = false;
            IsRegisterPasswordInvalid = false;
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            IsFirstNameInvalid = string.IsNullOrWhiteSpace(FirstName);
            IsLastNameInvalid = string.IsNullOrWhiteSpace(LastName);
            IsRegisterEmailInvalid = string.IsNullOrWhiteSpace(RegisterEmail);
            IsRegisterPasswordInvalid = string.IsNullOrWhiteSpace(RegisterPassword);

            if (IsFirstNameInvalid || IsLastNameInvalid || IsRegisterEmailInvalid || IsRegisterPasswordInvalid)
            {
                SnackbarBackground = new SolidColorBrush(Colors.Red);
                _notificationService.ShowWarning("Заполните все поля регистрации");
                return;
            }

            if (!IsRegisterEmailInvalid.Equals("@"))
            {
                SnackbarBackground = new SolidColorBrush(Colors.Red);
                _notificationService.ShowWarning("Почта должна быть правильного формата");
                return;
            }

            IsLoading = true;
            ErrorMessage = "";

            try
            {
                var dto = new Models.RegisterDto
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = RegisterEmail,
                    Password = RegisterPassword
                };

                var result = await _authService.RegisterAsync(dto);
                SnackbarBackground = new SolidColorBrush(Colors.Green);
                _notificationService.ShowSuccess($"Регистрация успешна. Войдите в аккаунт");
                Email = RegisterEmail;
                Password = string.Empty;
                RegisterPassword = string.Empty;
                ShowLogin();
            }
            catch (System.Exception ex)
            {
                SnackbarBackground = new SolidColorBrush(Colors.Red);
                _notificationService.ShowError(ex.Message);
                IsFirstNameInvalid = string.IsNullOrWhiteSpace(FirstName);
                IsLastNameInvalid = string.IsNullOrWhiteSpace(LastName);
                IsRegisterEmailInvalid = true;
                IsRegisterPasswordInvalid = string.IsNullOrWhiteSpace(RegisterPassword);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            IsEmailInvalid = string.IsNullOrWhiteSpace(Email);
            IsPasswordInvalid = string.IsNullOrWhiteSpace(Password);
            if (IsEmailInvalid || IsPasswordInvalid)
            {
                SnackbarBackground = new SolidColorBrush(Colors.Red);
                _notificationService.ShowWarning("Пожалуйста, введите email и пароль");
                return;
            }

            if (IsEmailInvalid.Equals("@"))
            {
                SnackbarBackground = new SolidColorBrush(Colors.Red);
                _notificationService.ShowWarning("Почта должна быть правильного формата");
                return;
            }

            IsLoading = true;
            ErrorMessage = "";

            try
            {
                var result = await _authService.LoginAsync(Email, Password);

                if (result != null)
                {
                    SnackbarBackground = new SolidColorBrush(Colors.Green);
                    _notificationService.ShowSuccess($"Добро пожаловать, {result.FirstName}!");

                    await Task.Delay(1000);
                    _navigationService.NavigateToDashboard();
                }
                else
                {
                    SnackbarBackground = new SolidColorBrush(Colors.Red);
                    _notificationService.ShowError("Неверный email или пароль");
                }
            }
            catch (System.Exception ex)
            {
                SnackbarBackground = new SolidColorBrush(Colors.Red);
                _notificationService.ShowError(ex.Message);
                if (ex.Message.Contains("Неверный email или пароль"))
                {
                    IsEmailInvalid = true;
                    IsPasswordInvalid = true;
                }
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
        private async void UseDemoUser()
        {
            try
            {
                Email = "dimaslizh@gmail.com";
                Password = "123123";
                PasswordChanged?.Invoke(Password);

            }
            catch (System.Exception ex)
            {
                if (ex.Message.Contains("Ошибка при загрузке данных"))
                {
                    IsEmailInvalid = true;
                    IsPasswordInvalid = true;
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnPasswordChanged(string value)
        {
            PasswordChanged?.Invoke(value);
        }

        partial void OnEmailChanged(string value)
        {
            IsEmailInvalid = false;
        }

        partial void OnFirstNameChanged(string value)
        {
            IsFirstNameInvalid = false;
        }

        partial void OnLastNameChanged(string value)
        {
            IsLastNameInvalid = false;
        }

        partial void OnRegisterEmailChanged(string value)
        {
            IsRegisterEmailInvalid = false;
        }

        partial void OnRegisterPasswordChanged(string value)
        {
            IsRegisterPasswordInvalid = false;
        }
    }
}