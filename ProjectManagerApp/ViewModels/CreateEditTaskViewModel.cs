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
        private int? _taskId;

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
        private bool _isLoading = false;

        [ObservableProperty]
        private bool _isEditMode = false;

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

        public async Task InitializeForProject(int projectId, string projectName)
        {
            try
            {
                IsLoading = true;
                
                await LoadAsync();
                
                var project = Projects.FirstOrDefault(p => p.Id == projectId);
                if (project != null)
                {
                    SelectedProject = project;
                }
                
                IsEditMode = false;
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

        public async Task InitializeForEdit(int taskId)
        {
            try
            {
                IsLoading = true;
                _taskId = taskId;
                
                await LoadAsync();
                
                var task = await _tasksService.GetTaskAsync(taskId);
                if (task != null)
                {
                    TaskTitle = task.Title;
                    TaskDescription = task.Description ?? string.Empty;
                    SelectedStatus = task.Status;
                    SelectedPriority = task.Priority;
                    PlannedHours = task.PlannedHours?.ToString() ?? string.Empty;
                    ActualHours = task.ActualHours?.ToString() ?? string.Empty;
                    
                    var project = Projects.FirstOrDefault(p => p.Id == task.ProjectId);
                    if (project != null)
                    {
                        SelectedProject = project;
                    }
                    
                    var assignee = Users.FirstOrDefault(u => u.Id == task.AssigneeId);
                    if (assignee != null)
                    {
                        SelectedAssignee = assignee;
                    }
                }
                
                IsEditMode = true;
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
            finally
            {
                IsLoading = false;
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
            CanSave = !string.IsNullOrWhiteSpace(TaskTitle) && 
                     TaskTitle.Trim().Length >= 2 && 
                     TaskTitle.Trim().Length <= 100 &&
                     SelectedProject != null && 
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
                await Task.Delay(2000);

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

                if (_taskId.HasValue)
                {
                    await _tasksService.UpdateTaskAsync(_taskId.Value, taskDto);
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

            if (string.IsNullOrWhiteSpace(TaskTitle))
            {
                errors.Add("Введите заголовок задачи");
            }
            else if (TaskTitle.Trim().Length < 2)
            {
                errors.Add("Заголовок задачи должен содержать минимум 2 символа");
            }
            else if (TaskTitle.Trim().Length > 100)
            {
                errors.Add("Заголовок задачи не должен превышать 100 символов");
            }

            if (!string.IsNullOrWhiteSpace(TaskDescription) && TaskDescription.Trim().Length > 1000)
            {
                errors.Add("Описание задачи не должно превышать 1000 символов");
            }

            if (SelectedProject == null)
            {
                errors.Add("Выберите проект");
            }

            if (!string.IsNullOrWhiteSpace(PlannedHours))
            {
                if (!decimal.TryParse(PlannedHours.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var planned))
                {
                    errors.Add("Введите корректное количество запланированных часов");
                }
                else if (planned < 0 || planned > 9999)
                {
                    errors.Add("Запланированные часы должны быть от 0 до 9999");
                }
            }

            if (!string.IsNullOrWhiteSpace(ActualHours))
            {
                if (!decimal.TryParse(ActualHours.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var actual))
                {
                    errors.Add("Введите корректное количество фактических часов");
                }
                else if (actual < 0 || actual > 9999)
                {
                    errors.Add("Фактические часы должны быть от 0 до 9999");
                }
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

            if (message.Contains("заголовок") && message.Contains("уже существует"))
            {
                return "Задача с таким заголовком уже существует";
            }

            if (message.Contains("заголовок") && message.Contains("обязательно"))
            {
                return "Заголовок задачи является обязательным полем";
            }

            if (message.Contains("проект") && message.Contains("не найден"))
            {
                return "Выбранный проект не найден";
            }

            if (message.Contains("проект") && message.Contains("обязательно"))
            {
                return "Проект является обязательным полем";
            }

            if (message.Contains("исполнитель") && message.Contains("не найден"))
            {
                return "Выбранный исполнитель не найден";
            }

            if (message.Contains("приоритет") && message.Contains("неверный"))
            {
                return "Выберите корректный приоритет задачи";
            }

            if (message.Contains("статус") && message.Contains("неверный"))
            {
                return "Выберите корректный статус задачи";
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

            return "Произошла ошибка при сохранении задачи. Проверьте введенные данные";
        }

    }
}

