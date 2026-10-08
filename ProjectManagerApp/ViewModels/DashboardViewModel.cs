using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

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
        private readonly IDataRefreshService _dataRefreshService;
        private DispatcherTimer _timer;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _currentDateTime = string.Empty;

        [ObservableProperty]
        private string _currentTime = string.Empty;

        [ObservableProperty]
        private string _currentDate = string.Empty;

        [ObservableProperty]
        private string _attentionSummary = string.Empty;

        [ObservableProperty]
        private int _totalProjects = 0;

        [ObservableProperty]
        private int _totalTasks = 0;

        [ObservableProperty]
        private int _totalUsers = 0;

        [ObservableProperty]
        private int _totalComments = 0;

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
        private int _taskStatusPostponed = 0;

        [ObservableProperty]
        private int _taskStatusProblem = 0;

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
            INotificationService notificationService,
            IDataRefreshService dataRefreshService)
        {
            _authService = authService;
            _permissionService = permissionService;
            _projectsService = projectsService;
            _tasksService = tasksService;
            _usersService = usersService;
            _statisticsService = statisticsService;
            _notificationService = notificationService;
            _dataRefreshService = dataRefreshService;
            
            InitializeTimer();
            UpdateCurrentDateTime();

            _dataRefreshService.Changed += OnDataRefresh;
        }

        private void OnDataRefresh(object? sender, DataRefreshScope scope)
        {
            if (scope == DataRefreshScope.None)
            {
                return;
            }

            System.Windows.Application.Current?.Dispatcher.BeginInvoke(() => _ = LoadDataAsync());
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
                TotalComments = overviewStats.TotalComments;

                ActiveProjects = overviewStats.ActiveProjects;

                CompletedTasks = overviewStats.CompletedTasks;
                ActiveTasks = overviewStats.ActiveTasks;

                TaskStatusNew = overviewStats.TaskStatusNew;
                TaskStatusInProgress = overviewStats.TaskStatusInProgress;
                TaskStatusReview = overviewStats.TaskStatusReview;
                TaskStatusCompleted = overviewStats.TaskStatusCompleted;
                TaskStatusPostponed = overviewStats.TaskStatusPostponed;
                TaskStatusProblem = overviewStats.TaskStatusProblem;

                TaskPriorityLow = overviewStats.TaskPriorityLow;
                TaskPriorityMedium = overviewStats.TaskPriorityMedium;
                TaskPriorityHigh = overviewStats.TaskPriorityHigh;
                TaskPriorityCritical = overviewStats.TaskPriorityCritical;

                ProjectStatusPlanned = overviewStats.ProjectStatusPlanned;
                ProjectStatusInProgress = overviewStats.ProjectStatusInProgress;
                ProjectStatusCompleted = overviewStats.ProjectStatusCompleted;

                UpdateAttentionSummary();
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
        private void ShowPermissionsMatrix()
        {
            PermissionMatrixHelper.Show();
        }

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) => UpdateCurrentDateTime();
            _timer.Start();
        }

        private void UpdateCurrentDateTime()
        {
            var now = DateTime.Now;
            var culture = new System.Globalization.CultureInfo("ru-RU");
            
            CurrentDateTime = now.ToString("dddd, dd MMMM yyyy, HH:mm:ss", culture);
            CurrentTime = now.ToString("HH:mm:ss", culture);
            CurrentDate = now.ToString("dddd, dd MMMM yyyy", culture);
        }

        private void UpdateAttentionSummary()
        {
            if (TaskStatusProblem > 0)
            {
                AttentionSummary = $"{TaskStatusProblem} задач с проблемой — проверьте раздел «Задачи»";
            }
            else if (TaskPriorityCritical > 0)
            {
                AttentionSummary = $"{TaskPriorityCritical} задач с критическим приоритетом";
            }
            else if (TaskStatusReview > 0)
            {
                AttentionSummary = $"{TaskStatusReview} задач ожидают проверки";
            }
            else if (TaskStatusNew > 0)
            {
                AttentionSummary = $"{TaskStatusNew} новых задач к распределению";
            }
            else
            {
                AttentionSummary = string.Empty;
            }
        }
    }
}
