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
            await LoadDataAsync();
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
            CanSave = !string.IsNullOrWhiteSpace(ProjectName) && SelectedManager != null && !IsSaving;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (!CanSave) return;

            IsSaving = true;
            CanSave = false;

            try
            {
                await System.Threading.Tasks.Task.Delay(1000);

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
                _notificationService.ShowError($"Ошибка сохранения проекта: {ex.Message}");
            }
            finally
            {
                IsSaving = false;
                ValidateForm();
            }
        }
    }
}

