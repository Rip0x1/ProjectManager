using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementSystem.WPF.ViewModels
{
    public partial class CreateEditProjectViewModel : ObservableObject
    {
        private readonly IProjectsService _projectsService;
        private readonly IUsersService _usersService;
        private readonly INotificationService _notificationService;
        private readonly int? _projectId;

        public event EventHandler<bool> CloseRequested;

        [ObservableProperty]
        private string _windowTitle = "Создание проекта";

        [ObservableProperty]
        private string _saveButtonText = "Создать";

        [ObservableProperty]
        private string _projectName = string.Empty;

        [ObservableProperty]
        private string _projectDescription = string.Empty;

        [ObservableProperty]
        private DateTime? _deadline;

        [ObservableProperty]
        private ObservableCollection<UserItem> _managers = new();

        [ObservableProperty]
        private UserItem _selectedManager;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private bool _isSaving = false;

        [ObservableProperty]
        private bool _canSave = false;

        public ObservableCollection<KeyValuePair<int, string>> ProjectStatuses { get; } = new()
        {
            new KeyValuePair<int, string>(0, "Активный"),
            new KeyValuePair<int, string>(1, "Завершен"),
            new KeyValuePair<int, string>(2, "Приостановлен")
        };

        [ObservableProperty]
        private KeyValuePair<int, string> _selectedStatus;

        public CreateEditProjectViewModel(
            IProjectsService projectsService,
            IUsersService usersService,
            INotificationService notificationService,
            int? projectId = null)
        {
            _projectsService = projectsService;
            _usersService = usersService;
            _notificationService = notificationService;
            _projectId = projectId;

            if (projectId.HasValue)
            {
                WindowTitle = "Редактирование проекта";
                SaveButtonText = "Сохранить";
            }

            _ = LoadDataAsync();
        }

        public async Task InitializeForEdit(int projectId)
        {
            try
            {
                IsLoading = true;
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка инициализации: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var users = await _usersService.GetUsersAsync();
                var managerUsers = users
                    .Where(u => u.Role == 1 || u.Role == 2) 
                    .Select(UsersService.MapToUserItem)
                    .ToList();

                Managers.Clear();
                foreach (var manager in managerUsers)
                {
                    Managers.Add(manager);
                }

                if (_projectId.HasValue)
                {
                    var project = await _projectsService.GetProjectAsync(_projectId.Value);
                    if (project != null)
                    {
                        ProjectName = project.Name;
                        ProjectDescription = project.Description;
                        Deadline = project.Deadline;
                        SelectedManager = Managers.FirstOrDefault(m => m.Id == project.ManagerId);
                        
                        var statusItem = ProjectStatuses.FirstOrDefault(s => s.Key == project.Status);
                        SelectedStatus = statusItem.Key != 0 || statusItem.Value != null ? statusItem : ProjectStatuses.First();
                    }
                }
                else
                {
                    if (Managers.Any())
                    {
                        SelectedManager = Managers.First();
                    }
                    SelectedStatus = ProjectStatuses.First();
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка загрузки данных: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnProjectNameChanged(string value)
        {
            ValidateForm();
        }

        partial void OnSelectedManagerChanged(UserItem value)
        {
            ValidateForm();
        }

        private void ValidateForm()
        {
            CanSave = !string.IsNullOrWhiteSpace(ProjectName) && 
                     ProjectName.Trim().Length >= 2 && 
                     ProjectName.Trim().Length <= 100 &&
                     SelectedManager != null && 
                     !IsSaving;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (!ValidateInput())
                return;

            IsSaving = true;
            CanSave = false;

            try
            {
                await System.Threading.Tasks.Task.Delay(2000);

                var projectDto = new CreateUpdateProjectDto
                {
                    Name = ProjectName.Trim(),
                    Description = ProjectDescription?.Trim() ?? string.Empty,
                    ManagerId = SelectedManager.Id,
                    Status = SelectedStatus.Key,
                    Deadline = Deadline
                };

                if (_projectId.HasValue)
                {
                    await _projectsService.UpdateProjectAsync(_projectId.Value, projectDto);
                    _notificationService.ShowSuccess("Проект успешно обновлен!");
                }
                else
                {
                    await _projectsService.CreateProjectAsync(projectDto);
                    _notificationService.ShowSuccess("Проект успешно создан!");
                }

                CloseRequested?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                var userFriendlyMessage = GetUserFriendlyErrorMessage(ex);
                _notificationService.ShowError(userFriendlyMessage);
            }
            finally
            {
                IsSaving = false;
                ValidateForm();
            }
        }

        private bool ValidateInput()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(ProjectName))
            {
                errors.Add("Введите название проекта");
            }
            else if (ProjectName.Trim().Length < 2)
            {
                errors.Add("Название проекта должно содержать минимум 2 символа");
            }
            else if (ProjectName.Trim().Length > 100)
            {
                errors.Add("Название проекта не должно превышать 100 символов");
            }

            if (!string.IsNullOrWhiteSpace(ProjectDescription) && ProjectDescription.Trim().Length > 500)
            {
                errors.Add("Описание проекта не должно превышать 500 символов");
            }

            if (SelectedManager == null)
            {
                errors.Add("Выберите менеджера проекта");
            }

            if (Deadline.HasValue && Deadline.Value < DateTime.Today)
            {
                errors.Add("Срок выполнения не может быть в прошлом");
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

            if (message.Contains("название") && message.Contains("уже существует"))
            {
                return "Проект с таким названием уже существует";
            }

            if (message.Contains("менеджер") && message.Contains("не найден"))
            {
                return "Выбранный менеджер не найден";
            }

            if (message.Contains("название") && message.Contains("обязательно"))
            {
                return "Название проекта является обязательным полем";
            }

            if (message.Contains("менеджер") && message.Contains("обязательно"))
            {
                return "Менеджер проекта является обязательным полем";
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

            return "Произошла ошибка при сохранении проекта. Проверьте введенные данные";
        }
    }
}

