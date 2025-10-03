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
        private readonly IStatisticsService _statisticsService;
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
            IStatisticsService statisticsService,
            INotificationService notificationService)
        {
            _authService = authService;
            _permissionService = permissionService;
            _projectsService = projectsService;
            _tasksService = tasksService;
            _usersService = usersService;
            _statisticsService = statisticsService;
            _notificationService = notificationService;
        }

        public async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                var overviewStats = await _statisticsService.GetOverviewStatisticsAsync();

                TotalProjects = overviewStats.TotalProjects;
                TotalTasks = overviewStats.TotalTasks;
                TotalUsers = overviewStats.TotalUsers;

                ActiveProjects = overviewStats.ActiveProjects;

                CompletedTasks = overviewStats.CompletedTasks;
                ActiveTasks = overviewStats.ActiveTasks;

                TaskStatusNew = overviewStats.TaskStatusNew;
                TaskStatusInProgress = overviewStats.TaskStatusInProgress;
                TaskStatusReview = overviewStats.TaskStatusReview;
                TaskStatusCompleted = overviewStats.TaskStatusCompleted;

                TaskPriorityLow = overviewStats.TaskPriorityLow;
                TaskPriorityMedium = overviewStats.TaskPriorityMedium;
                TaskPriorityHigh = overviewStats.TaskPriorityHigh;
                TaskPriorityCritical = overviewStats.TaskPriorityCritical;

                ProjectStatusPlanned = overviewStats.ProjectStatusPlanned;
                ProjectStatusInProgress = overviewStats.ProjectStatusInProgress;
                ProjectStatusCompleted = overviewStats.ProjectStatusCompleted;

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
