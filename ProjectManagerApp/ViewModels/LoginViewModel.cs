using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Services;
using System.Collections.Generic;
using System.Linq;
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
        private string _login = "";

        [ObservableProperty]
        private string _password = "";

        [ObservableProperty]
        private string _errorMessage = "";

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private bool _isLoginInvalid = false;

        [ObservableProperty]
        private bool _isPasswordInvalid = false;

        [ObservableProperty]
        private bool _isFirstNameInvalid = false;

        [ObservableProperty]
        private bool _isLastNameInvalid = false;

        [ObservableProperty]
        private bool _isRegisterLoginInvalid = false;

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
        private string _registerLogin = "";

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
            IsLoginInvalid = false;
            IsPasswordInvalid = false;
            IsFirstNameInvalid = false;
            IsLastNameInvalid = false;
            IsRegisterLoginInvalid = false;
            IsRegisterPasswordInvalid = false;
        }

        [RelayCommand]
        private void ShowLogin()
        {
            IsRegisterMode = false;
            IsLoginMode = true;
            ErrorMessage = "";
            IsLoginInvalid = false;
            IsPasswordInvalid = false;
            IsFirstNameInvalid = false;
            IsLastNameInvalid = false;
            IsRegisterLoginInvalid = false;
            IsRegisterPasswordInvalid = false;
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            if (!ValidateRegistrationInput())
            {
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
                    Login = RegisterLogin,
                    Password = RegisterPassword
                };

                var result = await _authService.RegisterAsync(dto);
                SnackbarBackground = new SolidColorBrush(Colors.Green);
                _notificationService.ShowSuccess($"Регистрация успешна. Войдите в аккаунт");
                Login = RegisterLogin;
                Password = string.Empty;
                RegisterPassword = string.Empty;
                ShowLogin();
            }
            catch (System.Exception ex)
            {
                SnackbarBackground = new SolidColorBrush(Colors.Red);
                _notificationService.ShowError(GetUserFriendlyErrorMessage(ex));
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (!ValidateLoginInput())
            {
                return;
            }

            IsLoading = true;
            ErrorMessage = "";

            try
            {
                var result = await _authService.LoginAsync(Login, Password);

                if (result != null)
                {
                    SnackbarBackground = new SolidColorBrush(Colors.Green);
                    _notificationService.ShowSuccess($"Добро пожаловать, {result.FirstName}!");

                    await Task.Delay(2500);
                    _navigationService.NavigateToDashboard();
                }
                else
                {
                    SnackbarBackground = new SolidColorBrush(Colors.Red);
                    _notificationService.ShowError("Неверный логин или пароль");
                }
            }
            catch (System.Exception ex)
            {
                SnackbarBackground = new SolidColorBrush(Colors.Red);
                _notificationService.ShowError(GetUserFriendlyErrorMessage(ex));
                if (ex.Message.Contains("Неверный логин или пароль"))
                {
                    IsLoginInvalid = true;
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

        partial void OnLoginChanged(string value)
        {
            IsLoginInvalid = false;
            var sanitized = InputValidation.SanitizeLogin(value);
            if (sanitized != value)
            {
                Login = sanitized;
            }
        }

        partial void OnPasswordChanged(string value)
        {
            var sanitized = InputValidation.SanitizePassword(value);
            if (sanitized != value)
            {
                Password = sanitized;
                return;
            }

            PasswordChanged?.Invoke(value);
        }

        partial void OnFirstNameChanged(string value)
        {
            IsFirstNameInvalid = false;
            var sanitized = InputValidation.SanitizePersonName(value);
            if (sanitized != value)
            {
                FirstName = sanitized;
            }
        }

        partial void OnLastNameChanged(string value)
        {
            IsLastNameInvalid = false;
            var sanitized = InputValidation.SanitizePersonName(value);
            if (sanitized != value)
            {
                LastName = sanitized;
            }
        }

        partial void OnRegisterLoginChanged(string value)
        {
            IsRegisterLoginInvalid = false;
            var sanitized = InputValidation.SanitizeLogin(value);
            if (sanitized != value)
            {
                RegisterLogin = sanitized;
            }
        }

        partial void OnRegisterPasswordChanged(string value)
        {
            IsRegisterPasswordInvalid = false;
            var sanitized = InputValidation.SanitizePassword(value);
            if (sanitized != value)
            {
                RegisterPassword = sanitized;
            }
        }

        private bool ValidateLoginInput()
        {
            var errors = new List<string>();
            errors.AddRange(InputValidation.ValidateLogin(Login));
            errors.AddRange(InputValidation.ValidatePassword(Password, required: true));

            IsLoginInvalid = errors.Any(e => e.Contains("логин", StringComparison.OrdinalIgnoreCase));
            IsPasswordInvalid = errors.Any(e => e.Contains("парол", StringComparison.OrdinalIgnoreCase));

            if (errors.Any())
            {
                SnackbarBackground = new SolidColorBrush(Colors.Red);
                _notificationService.ShowError(string.Join("\n", errors));
                return false;
            }

            IsLoginInvalid = false;
            IsPasswordInvalid = false;
            return true;
        }

        private bool ValidateRegistrationInput()
        {
            var errors = new List<string>();
            errors.AddRange(InputValidation.ValidatePersonName(FirstName, "Имя"));
            errors.AddRange(InputValidation.ValidatePersonName(LastName, "Фамилия"));
            errors.AddRange(InputValidation.ValidateLogin(RegisterLogin));
            errors.AddRange(InputValidation.ValidatePassword(RegisterPassword, required: true));

            IsFirstNameInvalid = errors.Any(e => e.StartsWith("Имя", StringComparison.OrdinalIgnoreCase) || e.Contains("имя", StringComparison.OrdinalIgnoreCase));
            IsLastNameInvalid = errors.Any(e => e.StartsWith("Фамилия", StringComparison.OrdinalIgnoreCase) || e.Contains("фамили", StringComparison.OrdinalIgnoreCase));
            IsRegisterLoginInvalid = errors.Any(e => e.Contains("логин", StringComparison.OrdinalIgnoreCase));
            IsRegisterPasswordInvalid = errors.Any(e => e.Contains("парол", StringComparison.OrdinalIgnoreCase));

            if (errors.Any())
            {
                SnackbarBackground = new SolidColorBrush(Colors.Red);
                _notificationService.ShowError(string.Join("\n", errors));
                return false;
            }

            IsFirstNameInvalid = false;
            IsLastNameInvalid = false;
            IsRegisterLoginInvalid = false;
            IsRegisterPasswordInvalid = false;
            return true;
        }

        private string GetUserFriendlyErrorMessage(Exception ex)
        {
            var message = ex.Message.ToLower();

            if (message.Contains("неверный логин или пароль") || message.Contains("неверный email или пароль"))
            {
                return "Неверный логин или пароль";
            }

            if ((message.Contains("логин") || message.Contains("email")) && message.Contains("уже существует"))
            {
                return "Пользователь с таким логином уже существует";
            }

            if ((message.Contains("логин") || message.Contains("email")) && message.Contains("некорректный"))
            {
                return "Введите корректный логин";
            }

            if (message.Contains("имя") && message.Contains("обязательно"))
            {
                return "Имя является обязательным полем";
            }

            if (message.Contains("фамилия") && message.Contains("обязательно"))
            {
                return "Фамилия является обязательным полем";
            }

            if (message.Contains("пароль") && message.Contains("короткий"))
            {
                return "Пароль должен содержать минимум 6 символов";
            }

            if (message.Contains("подключения к серверу") || message.Contains("включить api"))
            {
                return "Ошибка подключения к серверу. Проверьте, что API запущен";
            }

            if (message.Contains("некорректный запрос"))
            {
                return "Проверьте правильность введенных данных";
            }

            if (message.Contains("ошибка сервера"))
            {
                return "Временная ошибка сервера. Попробуйте позже";
            }

            return "Произошла ошибка. Проверьте введенные данные";
        }
    }
}