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
    public partial class CreateEditTaskViewModel : ObservableObject
    {
        private readonly ITasksService _tasksService;
        private readonly IProjectsService _projectsService;
        private readonly IUsersService _usersService;
        private readonly IAuthService _authService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private string _taskTitle = string.Empty;

        [ObservableProperty]
        private string _taskDescription = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ProjectItem> _projects = new();

        [ObservableProperty]
        private ProjectItem _selectedProject;

        [ObservableProperty]
        private ObservableCollection<UserItem> _users = new();

        [ObservableProperty]
        private UserItem? _selectedAssignee;

        [ObservableProperty]
        private int _selectedStatus = 0;

        [ObservableProperty]
        private int _selectedPriority = 1;

        [ObservableProperty]
        private string _plannedHours = string.Empty;

        [ObservableProperty]
        private string _actualHours = string.Empty;

        [ObservableProperty]
        private bool _isSaving = false;

        [ObservableProperty]
        private bool _canSave = false;

        public int? TaskId { get; set; }

        public string WindowTitle => TaskId.HasValue ? "Редактирование задачи" : "Создание задачи";
        public string SaveButtonText => TaskId.HasValue ? "Сохранить" : "Создать";

        public event EventHandler<bool>? CloseRequested;

        public CreateEditTaskViewModel(
            ITasksService tasksService,
            IProjectsService projectsService,
            IUsersService usersService,
            IAuthService authService,
            INotificationService notificationService)
        {
            _tasksService = tasksService;
            _projectsService = projectsService;
            _usersService = usersService;
            _authService = authService;
            _notificationService = notificationService;
        }

        public async Task LoadAsync()
        {
            try
            {
                var projectsList = await _projectsService.GetProjectsAsync();
                Projects = new ObservableCollection<ProjectItem>(projectsList);

                var usersListDto = await _usersService.GetUsersAsync();
                var usersList = usersListDto.Select(u => new UserItem
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt
                });
                Users = new ObservableCollection<UserItem>(usersList);

                if (TaskId.HasValue)
                {
                    var task = await _tasksService.GetTaskAsync(TaskId.Value);
                    if (task != null)
                    {
                        TaskTitle = task.Title;
                        TaskDescription = task.Description;
                        SelectedStatus = task.Status;
                        SelectedPriority = task.Priority;
                        PlannedHours = task.PlannedHours?.ToString() ?? string.Empty;
                        ActualHours = task.ActualHours?.ToString() ?? string.Empty;
                        
                        SelectedProject = Projects.FirstOrDefault(p => p.Id == task.ProjectId);
                        SelectedAssignee = Users.FirstOrDefault(u => u.Id == task.AssigneeId);
                    }
                }
                else
                {
                    if (Projects.Any())
                    {
                        SelectedProject = Projects.First();
                    }
                }

                ValidateForm();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        partial void OnTaskTitleChanged(string value)
        {
            ValidateForm();
        }

        partial void OnSelectedProjectChanged(ProjectItem value)
        {
            ValidateForm();
        }

        private void ValidateForm()
        {
            CanSave = !string.IsNullOrWhiteSpace(TaskTitle) && SelectedProject != null && !IsSaving;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (!CanSave) return;

            IsSaving = true;
            CanSave = false;

            try
            {
                await Task.Delay(500);

                decimal? plannedHrs = null;
                if (!string.IsNullOrWhiteSpace(PlannedHours))
                {
                    if (decimal.TryParse(PlannedHours.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var planned))
                    {
                        plannedHrs = planned;
                    }
                }

                decimal? actualHrs = null;
                if (!string.IsNullOrWhiteSpace(ActualHours))
                {
                    if (decimal.TryParse(ActualHours.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var actual))
                    {
                        actualHrs = actual;
                    }
                }

                var taskDto = new CreateUpdateTaskDto
                {
                    Title = TaskTitle.Trim(),
                    Description = TaskDescription?.Trim() ?? string.Empty,
                    Status = SelectedStatus,
                    Priority = SelectedPriority,
                    ProjectId = SelectedProject.Id,
                    AuthorId = _authService.CurrentUserId,
                    AssigneeId = SelectedAssignee?.Id,
                    PlannedHours = plannedHrs,
                    ActualHours = actualHrs
                };

                if (TaskId.HasValue)
                {
                    await _tasksService.UpdateTaskAsync(TaskId.Value, taskDto);
                    _notificationService.ShowSuccess("Задача успешно обновлена!");
                }
                else
                {
                    await _tasksService.CreateTaskAsync(taskDto);
                    _notificationService.ShowSuccess("Задача успешно создана!");
                }

                CloseRequested?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка сохранения задачи: {ex.Message}");
            }
            finally
            {
                IsSaving = false;
                ValidateForm();
            }
        }
    }
}

