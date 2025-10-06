using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
                var result = await _authService.LoginAsync(Email, Password);

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
                    _notificationService.ShowError("Неверный email или пароль");
                }
            }
            catch (System.Exception ex)
            {
                SnackbarBackground = new SolidColorBrush(Colors.Red);
                _notificationService.ShowError(GetUserFriendlyErrorMessage(ex));
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

        //[RelayCommand]
        //private async void UseDemoUser()
        //{
        //    try
        //    {
        //        Email = "dimaslizh@gmail.com";
        //        Password = "123123";
        //        PasswordChanged?.Invoke(Password);

        //    }
        //    catch (System.Exception ex)
        //    {
        //        if (ex.Message.Contains("Ошибка при загрузке данных"))
        //        {
        //            IsEmailInvalid = true;
        //            IsPasswordInvalid = true;
        //        }
        //    }
        //    finally
        //    {
        //        IsLoading = false;
        //    }
        //}

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

        private bool ValidateLoginInput()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Email))
            {
                errors.Add("Введите email");
                IsEmailInvalid = true;
            }
            else if (!IsValidEmail(Email))
            {
                errors.Add("Введите корректный email адрес");
                IsEmailInvalid = true;
            }
            else
            {
                IsEmailInvalid = false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                errors.Add("Введите пароль");
                IsPasswordInvalid = true;
            }
            else
            {
                IsPasswordInvalid = false;
            }

            if (errors.Any())
            {
                SnackbarBackground = new SolidColorBrush(Colors.Red);
                var errorMessage = string.Join("\n", errors);
                System.Diagnostics.Debug.WriteLine($"Login validation error: {errorMessage}");
                _notificationService.ShowError(errorMessage);
                return false;
            }

            return true;
        }

        private bool ValidateRegistrationInput()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(FirstName))
            {
                errors.Add("Введите имя");
                IsFirstNameInvalid = true;
            }
            else if (FirstName.Trim().Length < 2)
            {
                errors.Add("Имя должно содержать минимум 2 символа");
                IsFirstNameInvalid = true;
            }
            else if (FirstName.Trim().Length > 50)
            {
                errors.Add("Имя не должно превышать 50 символов");
                IsFirstNameInvalid = true;
            }
            else
            {
                IsFirstNameInvalid = false;
            }

            if (string.IsNullOrWhiteSpace(LastName))
            {
                errors.Add("Введите фамилию");
                IsLastNameInvalid = true;
            }
            else if (LastName.Trim().Length < 2)
            {
                errors.Add("Фамилия должна содержать минимум 2 символа");
                IsLastNameInvalid = true;
            }
            else if (LastName.Trim().Length > 50)
            {
                errors.Add("Фамилия не должна превышать 50 символов");
                IsLastNameInvalid = true;
            }
            else
            {
                IsLastNameInvalid = false;
            }

            if (string.IsNullOrWhiteSpace(RegisterEmail))
            {
                errors.Add("Введите email");
                IsRegisterEmailInvalid = true;
            }
            else if (!IsValidEmail(RegisterEmail))
            {
                errors.Add("Введите корректный email адрес");
                IsRegisterEmailInvalid = true;
            }
            else
            {
                IsRegisterEmailInvalid = false;
            }

            if (string.IsNullOrWhiteSpace(RegisterPassword))
            {
                errors.Add("Введите пароль");
                IsRegisterPasswordInvalid = true;
            }
            else if (RegisterPassword.Length < 6)
            {
                errors.Add("Пароль должен содержать минимум 6 символов");
                IsRegisterPasswordInvalid = true;
            }
            else if (RegisterPassword.Length > 100)
            {
                errors.Add("Пароль не должен превышать 100 символов");
                IsRegisterPasswordInvalid = true;
            }
            else
            {
                IsRegisterPasswordInvalid = false;
            }

            if (errors.Any())
            {
                SnackbarBackground = new SolidColorBrush(Colors.Red);
                var errorMessage = string.Join("\n", errors);
                System.Diagnostics.Debug.WriteLine($"Registration validation error: {errorMessage}");
                _notificationService.ShowError(errorMessage);
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private string GetUserFriendlyErrorMessage(Exception ex)
        {
            var message = ex.Message.ToLower();

            if (message.Contains("неверный email или пароль"))
            {
                return "Неверный email или пароль";
            }

            if (message.Contains("email") && message.Contains("уже существует"))
            {
                return "Пользователь с таким email уже существует";
            }

            if (message.Contains("email") && message.Contains("некорректный"))
            {
                return "Введите корректный email адрес";
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