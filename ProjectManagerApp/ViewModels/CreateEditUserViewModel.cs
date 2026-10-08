using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;
using System.Collections.ObjectModel;
using System.Windows;
using ProjectManagerApp.Views;
using System.Diagnostics;

namespace ProjectManagerApp.ViewModels
{
    public partial class CreateEditUserViewModel : ObservableObject
    {
        private readonly IUsersService _usersService;
        private readonly INotificationService _notificationService;
        private readonly int? _userId;

        [ObservableProperty]
        private string _windowTitle = "Создать пользователя";

        [ObservableProperty]
        private string _saveButtonText = "Создать";

        [ObservableProperty]
        private string _firstName = string.Empty;

        [ObservableProperty]
        private string _lastName = string.Empty;

        [ObservableProperty]
        private string _login = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _confirmPassword = string.Empty;

        [ObservableProperty]
        private int _selectedRole = 0; 

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private bool _isSaving = false;

        [ObservableProperty]
        private bool _canSave = false;

        public ObservableCollection<KeyValuePair<int, string>> Roles { get; } = new()
        {
            new KeyValuePair<int, string>(0, "Пользователь"),
            new KeyValuePair<int, string>(1, "Менеджер"),
            new KeyValuePair<int, string>(2, "Администратор")
        };

        public Task? InitialLoadTask { get; }

        public bool IsEditMode => _userId.HasValue;

        public CreateEditUserViewModel(IUsersService usersService, INotificationService notificationService, int? userId = null)
        {
            _usersService = usersService;
            _notificationService = notificationService;
            _userId = userId;

            if (IsEditMode)
            {
                WindowTitle = "Редактирование пользователя";
                SaveButtonText = "Обновить";
                InitialLoadTask = LoadUserAsync();
            }

            ValidateForm();
        }

        partial void OnFirstNameChanged(string value)
        {
            var sanitized = InputValidation.SanitizePersonName(value);
            if (sanitized != value)
            {
                FirstName = sanitized;
                return;
            }

            ValidateForm();
        }

        partial void OnLastNameChanged(string value)
        {
            var sanitized = InputValidation.SanitizePersonName(value);
            if (sanitized != value)
            {
                LastName = sanitized;
                return;
            }

            ValidateForm();
        }

        partial void OnLoginChanged(string value)
        {
            var sanitized = InputValidation.SanitizeLogin(value);
            if (sanitized != value)
            {
                Login = sanitized;
                return;
            }

            ValidateForm();
        }

        partial void OnPasswordChanged(string value)
        {
            var sanitized = InputValidation.SanitizePassword(value);
            if (sanitized != value)
            {
                Password = sanitized;
                return;
            }

            ValidateForm();
        }

        partial void OnConfirmPasswordChanged(string value)
        {
            var sanitized = InputValidation.SanitizePassword(value);
            if (sanitized != value)
            {
                ConfirmPassword = sanitized;
                return;
            }

            ValidateForm();
        }

        partial void OnSelectedRoleChanged(int value) => ValidateForm();

        partial void OnIsSavingChanged(bool value) => ValidateForm();

        private void ValidateForm()
        {
            var hasNames = !string.IsNullOrWhiteSpace(FirstName) &&
                           FirstName.Trim().Length >= ValidationLimits.PersonNameMin &&
                           !string.IsNullOrWhiteSpace(LastName) &&
                           LastName.Trim().Length >= ValidationLimits.PersonNameMin;
            var hasLogin = InputValidation.IsValidLogin(Login);

            if (!IsEditMode)
            {
                CanSave = hasNames &&
                          hasLogin &&
                          !string.IsNullOrWhiteSpace(Password) &&
                          Password.Length >= ValidationLimits.PasswordMin &&
                          Password == ConfirmPassword &&
                          !IsSaving;
                return;
            }

            CanSave = hasNames &&
                      hasLogin &&
                      (string.IsNullOrWhiteSpace(Password) ||
                       Password.Length >= ValidationLimits.PasswordMin) &&
                      !IsSaving;
        }

        private async Task LoadUserAsync()
        {
            try
            {
                IsLoading = true;
                var user = await _usersService.GetUserAsync(_userId!.Value);
                if (user != null)
                {
                    FirstName = user.FirstName;
                    LastName = user.LastName;
                    Login = user.Login;
                    SelectedRole = user.Role;
                    Password = string.Empty;
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка загрузки пользователя: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (!ValidateInput())
                return;

            try
            {
                IsLoading = true;
                IsSaving = true;
                await Task.Delay(2000);

                if (IsEditMode)
                {
                    var updateDto = new UpdateUserDto
                    {
                        FirstName = FirstName.Trim(),
                        LastName = LastName.Trim(),
                        Login = Login.Trim(),
                        Role = SelectedRole
                    };

                    if (!string.IsNullOrWhiteSpace(Password))
                    {
                        updateDto.Password = Password.Trim();
                    }

                    await _usersService.UpdateUserAsync(_userId!.Value, updateDto);
                }
                else
                {
                    var createDto = new CreateUserDto
                    {
                        FirstName = FirstName.Trim(),
                        LastName = LastName.Trim(),
                        Login = Login.Trim(),
                        Password = Password.Trim(),
                        Role = SelectedRole
                    };

                    await _usersService.CreateUserAsync(createDto);
                }

                ProjectManagementSystem.WPF.App.GetService<IDataRefreshService>()
                    .Notify(DataRefreshScopes.UserRelated);

                if (Application.Current.Windows.OfType<CreateEditUserWindow>().FirstOrDefault() is { } window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                var userFriendlyMessage = GetUserFriendlyErrorMessage(ex);
                _notificationService.ShowError(userFriendlyMessage);
            }
            finally
            {
                IsSaving = false;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            if (Application.Current.Windows.OfType<CreateEditUserWindow>().FirstOrDefault() is { } window)
            {
                window.DialogResult = false;
                window.Close();
            }
        }

        private bool ValidateInput()
        {
            var errors = new List<string>();
            errors.AddRange(InputValidation.ValidatePersonName(FirstName, "Имя"));
            errors.AddRange(InputValidation.ValidatePersonName(LastName, "Фамилия"));
            errors.AddRange(InputValidation.ValidateLogin(Login));

            if (!IsEditMode)
            {
                errors.AddRange(InputValidation.ValidatePasswordConfirmation(Password, ConfirmPassword, required: true));
            }
            else if (!string.IsNullOrWhiteSpace(Password))
            {
                errors.AddRange(InputValidation.ValidatePassword(Password, required: true));
            }

            if (errors.Any())
            {
                _notificationService.ShowError(string.Join("\n", errors));
                return false;
            }

            return true;
        }

        private string GetUserFriendlyErrorMessage(Exception ex)
        {
            var message = ex.Message.ToLower();

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

            if (message.Contains("роль") && message.Contains("неверная"))
            {
                return "Выберите корректную роль пользователя";
            }

            if (message.Contains("некорректный запрос"))
            {
                return "Проверьте правильность введенных данных";
            }

            if (message.Contains("ошибка сервера"))
            {
                return "Временная ошибка сервера. Попробуйте позже";
            }

            if (message.Contains("недостаточно прав"))
            {
                return "У вас недостаточно прав для выполнения этой операции";
            }

            return "Произошла ошибка при сохранении пользователя. Проверьте введенные данные";
        }
    }
}
