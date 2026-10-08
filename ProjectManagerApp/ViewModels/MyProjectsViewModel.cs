using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManagerApp.Models;
using ProjectManagerApp.Services;
using ProjectManagerApp.Views;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;
using System.Windows;

namespace ProjectManagerApp.ViewModels
{
    public partial class MyProjectsViewModel : ObservableObject
    {
        private readonly IUserProjectsService _userProjectsService;
        private readonly IAuthService _authService;
        private readonly INotificationService _notificationService;
        private readonly IApiClient _apiClient;
        private readonly ITasksService _tasksService;
        private readonly IProjectsService _projectsService;
        private readonly IUsersService _usersService;
        private readonly IDataRefreshService _dataRefreshService;
        private readonly IPermissionService _permissionService;

        [ObservableProperty]
        private ObservableCollection<UserProjectItem> _userProjects = new();

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _totalPages = 1;

        [ObservableProperty]
        private int _totalCount = 0;

        [ObservableProperty]
        private bool _canGoToPreviousPage;

        [ObservableProperty]
        private bool _canGoToNextPage;

        private const int PageSize = 10;

        public ObservableCollection<ColumnVisibilityOption> ColumnOptions { get; } = new()
        {
            new ColumnVisibilityOption("Id", "ID проекта"),
            new ColumnVisibilityOption("Name", "Название"),
            new ColumnVisibilityOption("Status", "Статус"),
            new ColumnVisibilityOption("RoleInProject", "Роль"),
            new ColumnVisibilityOption("ManagerName", "Менеджер"),
            new ColumnVisibilityOption("DeadlineText", "Дедлайн"),
            new ColumnVisibilityOption("ProgressText", "Прогресс"),
            new ColumnVisibilityOption("ParticipantsCount", "Участники"),
            new ColumnVisibilityOption("TasksCount", "Задачи"),
            new ColumnVisibilityOption("CommentsCount", "Комментарии"),
            new ColumnVisibilityOption("Actions", "Действия", isLocked: true)
        };

        public bool CanManageProjectMembers => _permissionService.IsManagerOrAbove();

        public MyProjectsViewModel(
            IUserProjectsService userProjectsService,
            IAuthService authService,
            INotificationService notificationService,
            IApiClient apiClient,
            ITasksService tasksService,
            IProjectsService projectsService,
            IUsersService usersService,
            IDataRefreshService dataRefreshService,
            IPermissionService permissionService)
        {
            _userProjectsService = userProjectsService;
            _authService = authService;
            _notificationService = notificationService;
            _apiClient = apiClient;
            _tasksService = tasksService;
            _projectsService = projectsService;
            _usersService = usersService;
            _dataRefreshService = dataRefreshService;
            _permissionService = permissionService;

            _dataRefreshService.Changed += OnDataRefresh;
        }

        private void OnDataRefresh(object? sender, DataRefreshScope scope)
        {
            if (!scope.HasFlag(DataRefreshScope.MyProjects))
            {
                return;
            }

            Application.Current?.Dispatcher.BeginInvoke(() => _ = LoadUserProjectsAsync());
        }

        public async Task LoadAsync()
        {
            await LoadUserProjectsAsync();
        }

        private async Task LoadUserProjectsAsync()
        {
            try
            {
                IsLoading = true;
                var userId = _authService.CurrentUserId;
                
                var projects = await _userProjectsService.GetUserProjectsAsync(userId);
                
                UserProjects.Clear();
                TotalCount = projects.Count;
                TotalPages = (int)System.Math.Ceiling((double)TotalCount / PageSize);
                
                
                UpdatePaginationButtons();
                
                var filteredProjects = ApplySearchAndPagination(projects);
                foreach (var project in filteredProjects)
                {
                    UserProjects.Add(project);
                }
            }
            catch (System.Exception ex)
            {
                _notificationService.ShowError($"Ошибка загрузки проектов: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private System.Collections.Generic.List<UserProjectItem> ApplySearchAndPagination(System.Collections.Generic.IList<UserProjectItem> projects)
        {
            var filtered = projects.AsEnumerable();

            if (!string.IsNullOrEmpty(SearchText))
            {
                var term = SearchText.Trim();
                filtered = filtered.Where(p =>
                    SearchHelper.MatchesId(p.Id, term) ||
                    SearchHelper.MatchesText(p.Name, term) ||
                    SearchHelper.MatchesText(p.Description, term) ||
                    SearchHelper.MatchesText(p.ManagerName, term));
            }

            var filteredList = filtered.ToList();
            TotalCount = filteredList.Count;
            TotalPages = (int)System.Math.Ceiling((double)TotalCount / PageSize);
            
            UpdatePaginationButtons();

            return filteredList
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            CurrentPage = 1;
            await LoadUserProjectsAsync();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadUserProjectsAsync();
            _notificationService.ShowSuccess("Список проектов обновлен");
        }

        [RelayCommand]
        private async void AddCommentAsync(UserProjectItem project)
        {
            try
            {
                var window = new ProjectManagerApp.Views.ProjectCommentsViewWindow();
                var viewModel = ProjectManagementSystem.WPF.App.GetService<ProjectCommentsViewViewModel>();
                
                await viewModel.Initialize(project.Id, project.Name, project.ManagerId);
                window.DataContext = viewModel;
                window.Owner = Application.Current.MainWindow;
                WindowFitHelper.ApplySizeBeforeShow(window, 920, 680);
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка открытия комментариев: {ex.Message}");
            }
        }

        [RelayCommand]
        private async void CreateTaskAsync(UserProjectItem project)
        {
            try
            {
                var window = new ProjectManagerApp.Views.ProjectTasksWindow();
                var viewModel = new ProjectManagerApp.ViewModels.ProjectTasksViewModel(
                    _tasksService,
                    _projectsService,
                    _usersService,
                    _authService,
                    _notificationService,
                    _apiClient
                );
                
                await viewModel.Initialize(project.Id, project.Name);
                window.DataContext = viewModel;
                window.Owner = Application.Current.MainWindow;
                WindowFitHelper.ApplySizeBeforeShow(window, 920, 740);
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка открытия задач проекта: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ManageMembersAsync(UserProjectItem project)
        {
            if (!_permissionService.CanEditProject(project.ManagerId))
            {
                _notificationService.ShowWarning("У вас нет прав для управления участниками этого проекта");
                return;
            }

            try
            {
                var window = new ProjectMembersViewWindow();
                var viewModel = ProjectManagementSystem.WPF.App.GetService<ProjectMembersViewViewModel>();
                await viewModel.InitializeAsync(project.Id, project.Name);
                window.DataContext = viewModel;
                window.Owner = Application.Current.MainWindow;
                WindowFitHelper.ApplySizeBeforeShow(window, 920, 680);
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка управления участниками: {ex.Message}");
            }
        }

        [RelayCommand]
        private async void ProjectSettingsAsync(UserProjectItem project)
        {
            try
            {
                var viewModel = new ProjectManagementSystem.WPF.ViewModels.CreateEditProjectViewModel(
                    _projectsService,
                    _usersService,
                    _notificationService,
                    project.Id
                );
                
                await viewModel.InitializeForEdit(project.Id);

                var window = new ProjectManagementSystem.WPF.Views.CreateEditProjectWindow(viewModel)
                {
                    Owner = Application.Current.MainWindow
                };
                WindowFitHelper.ApplySizeBeforeShow(window, 600, 620);
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка открытия настроек проекта: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadUserProjectsAsync();
            }
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await LoadUserProjectsAsync();
            }
        }

        [RelayCommand]
        private async Task FirstPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage = 1;
                await LoadUserProjectsAsync();
            }
        }

        [RelayCommand]
        private async Task LastPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage = TotalPages;
                await LoadUserProjectsAsync();
            }
        }

        private void UpdatePaginationButtons()
        {
            CanGoToPreviousPage = CurrentPage > 1;
            CanGoToNextPage = CurrentPage < TotalPages;
        }

        partial void OnSearchTextChanged(string value)
        {
            var sanitized = InputValidation.SanitizeSearch(value);
            if (sanitized != value)
            {
                SearchText = sanitized;
                return;
            }

            CurrentPage = 1;
            _ = LoadUserProjectsAsync();
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SearchText = string.Empty;
            CurrentPage = 1;
            _ = LoadUserProjectsAsync();
            _notificationService.ShowInfo("Фильтры сброшены");
        }

        partial void OnCurrentPageChanged(int value)
        {
            var clamped = InputValidation.ClampPage(value, TotalPages);
            if (clamped != value)
            {
                CurrentPage = clamped;
            }
        }

        public bool HasNoProjects => !IsLoading && TotalCount == 0;

        partial void OnIsLoadingChanged(bool value)
        {
            OnPropertyChanged(nameof(HasNoProjects));
        }

        partial void OnTotalCountChanged(int value)
        {
            OnPropertyChanged(nameof(HasNoProjects));
        }
    }
}
