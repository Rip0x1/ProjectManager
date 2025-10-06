using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        private string _email = string.Empty;

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


        public ObservableCollection<KeyValuePair<int, string>> Roles { get; } = new()
        {
            new KeyValuePair<int, string>(0, "Пользователь"),
            new KeyValuePair<int, string>(1, "Менеджер"),
            new KeyValuePair<int, string>(2, "Администратор")
        };

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
                _ = LoadUserAsync();
            }
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
                    Email = user.Email;
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
                        Email = Email.Trim(),
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
                        Email = Email.Trim(),
                        Password = Password.Trim(),
                        Role = SelectedRole
                    };

                    await _usersService.CreateUserAsync(createDto);
                }

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

            if (string.IsNullOrWhiteSpace(FirstName))
            {
                errors.Add("Введите имя");
            }
            else if (FirstName.Trim().Length < 2)
            {
                errors.Add("Имя должно содержать минимум 2 символа");
            }
            else if (FirstName.Trim().Length > 50)
            {
                errors.Add("Имя не должно превышать 50 символов");
            }

            if (string.IsNullOrWhiteSpace(LastName))
            {
                errors.Add("Введите фамилию");
            }
            else if (LastName.Trim().Length < 2)
            {
                errors.Add("Фамилия должна содержать минимум 2 символа");
            }
            else if (LastName.Trim().Length > 50)
            {
                errors.Add("Фамилия не должна превышать 50 символов");
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                errors.Add("Введите email");
            }
            else if (!IsValidEmail(Email))
            {
                errors.Add("Введите корректный email адрес");
            }
            else if (Email.Trim().Length > 100)
            {
                errors.Add("Email не должен превышать 100 символов");
            }

            if (!IsEditMode && string.IsNullOrWhiteSpace(Password))
            {
                errors.Add("Введите пароль");
            }

            if (!string.IsNullOrWhiteSpace(Password))
            {
                if (Password.Length < 6)
                {
                    errors.Add("Пароль должен содержать минимум 6 символов");
                }
                else if (Password.Length > 100)
                {
                    errors.Add("Пароль не должен превышать 100 символов");
                }
            }

            if (errors.Any())
            {
                _notificationService.ShowError(string.Join("\n", errors));
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
