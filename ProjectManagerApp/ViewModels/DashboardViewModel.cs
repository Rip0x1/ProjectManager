using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementSystem.WPF.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly IPermissionService _permissionService;
        private readonly IProjectsService _projectsService;
        private readonly ITasksService _tasksService;
        private readonly IUsersService _usersService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private int _totalProjects = 0;

        [ObservableProperty]
        private int _totalTasks = 0;

        [ObservableProperty]
        private int _totalUsers = 0;

        [ObservableProperty]
        private int _activeProjects = 0;

        [ObservableProperty]
        private int _completedTasks = 0;

        [ObservableProperty]
        private int _activeTasks = 0;

        [ObservableProperty]
        private int _taskStatusNew = 0;

        [ObservableProperty]
        private int _taskStatusInProgress = 0;

        [ObservableProperty]
        private int _taskStatusReview = 0;

        [ObservableProperty]
        private int _taskStatusCompleted = 0;

        [ObservableProperty]
        private int _taskPriorityLow = 0;

        [ObservableProperty]
        private int _taskPriorityMedium = 0;

        [ObservableProperty]
        private int _taskPriorityHigh = 0;

        [ObservableProperty]
        private int _taskPriorityCritical = 0;

        [ObservableProperty]
        private int _projectStatusPlanned = 0;

        [ObservableProperty]
        private int _projectStatusInProgress = 0;

        [ObservableProperty]
        private int _projectStatusCompleted = 0;

        public string RoleName => ((UserRole)_authService.CurrentUserRole).GetRoleName();
        public string RoleColor => ((UserRole)_authService.CurrentUserRole).GetRoleColor();

        public DashboardViewModel(
            IAuthService authService, 
            IPermissionService permissionService,
            IProjectsService projectsService,
            ITasksService tasksService,
            IUsersService usersService,
            INotificationService notificationService)
        {
            _authService = authService;
            _permissionService = permissionService;
            _projectsService = projectsService;
            _tasksService = tasksService;
            _usersService = usersService;
            _notificationService = notificationService;
        }

        public async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                var projectsTask = _projectsService.GetProjectsAsync();
                var tasksTask = _tasksService.GetTasksAsync();
                var usersTask = _usersService.GetUsersAsync();

                await Task.WhenAll(projectsTask, tasksTask, usersTask);

                var projects = projectsTask.Result;
                var tasks = tasksTask.Result;
                var users = usersTask.Result;

                TotalProjects = projects.Count;
                TotalTasks = tasks.Count();
                TotalUsers = users.Count();

                ActiveProjects = projects.Count(p => p.Status == 1);

                CompletedTasks = tasks.Count(t => t.Status == 3);
                ActiveTasks = tasks.Count(t => t.Status == 1);

                TaskStatusNew = tasks.Count(t => t.Status == 0);
                TaskStatusInProgress = tasks.Count(t => t.Status == 1);
                TaskStatusReview = tasks.Count(t => t.Status == 2);
                TaskStatusCompleted = tasks.Count(t => t.Status == 3);

                TaskPriorityLow = tasks.Count(t => t.Priority == 0);
                TaskPriorityMedium = tasks.Count(t => t.Priority == 1);
                TaskPriorityHigh = tasks.Count(t => t.Priority == 2);
                TaskPriorityCritical = tasks.Count(t => t.Priority == 3);

                ProjectStatusPlanned = projects.Count(p => p.Status == 0);
                ProjectStatusInProgress = projects.Count(p => p.Status == 1);
                ProjectStatusCompleted = projects.Count(p => p.Status == 2);

            }
            catch (System.Exception ex)
            {
                _notificationService.ShowError($"Ошибка загрузки данных: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task Refresh()
        {
            await LoadDataAsync();
            _notificationService.ShowSuccess($"Статистика обновлена!");
        }
    }
}
