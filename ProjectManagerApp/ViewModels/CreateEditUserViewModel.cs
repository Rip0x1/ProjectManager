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
                await Task.Delay(1000);

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
                _notificationService.ShowError($"Ошибка сохранения пользователя: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
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
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                _notificationService.ShowError("Введите имя");
                return false;
            }

            if (string.IsNullOrWhiteSpace(LastName))
            {
                _notificationService.ShowError("Введите фамилию");
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                _notificationService.ShowError("Введите email");
                return false;
            }

            if (!IsValidEmail(Email))
            {
                _notificationService.ShowError("Введите корректный email адрес");
                return false;
            }

            if (!IsEditMode && string.IsNullOrWhiteSpace(Password))
            {
                _notificationService.ShowError("Введите пароль");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Password) && Password.Length < 6)
            {
                _notificationService.ShowError("Пароль должен содержать минимум 6 символов");
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
    }
}
