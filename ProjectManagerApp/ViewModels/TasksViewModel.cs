using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;
using ProjectManagerApp.Services;
using ProjectManagerApp.ViewModels;
using ProjectManagerApp.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Linq;
using System.Windows;

namespace ProjectManagementSystem.WPF.ViewModels
{
    public partial class TasksViewModel : ObservableObject
    {
        private readonly ITasksService _tasksService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IUsersService _usersService;
        private readonly IDataRefreshService _dataRefreshService;
        private string _sortColumn = "Id";
        private ListSortDirection _sortDirection = ListSortDirection.Descending;

        [ObservableProperty]
        private ObservableCollection<TaskItem> _tasks = new ObservableCollection<TaskItem>();

        [ObservableProperty]
        private ObservableCollection<TaskItem> _pagedTasks = new ObservableCollection<TaskItem>();

        [ObservableProperty]
        private ObservableCollection<UserItem> _assigneeFilterUsers = new();

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private int _pageSize = 10;

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _totalPages = 1;

        [ObservableProperty]
        private int _totalItems = 0;

        [ObservableProperty]
        private bool _canManageTasks = false;

        [ObservableProperty]
        private bool _canCreateTasks = false;

        [ObservableProperty]
        private int _selectedStatusFilter = -1; 

        [ObservableProperty]
        private int _selectedPriorityFilter = -1;

        [ObservableProperty]
        private UserItem? _selectedAssigneeFilterUser;

        [ObservableProperty]
        private DateTime? _filterDueFrom;

        [ObservableProperty]
        private DateTime? _filterDueTo;

        public ObservableCollection<ColumnVisibilityOption> ColumnOptions { get; } = new()
        {
            new ColumnVisibilityOption("Id", "ID задачи"),
            new ColumnVisibilityOption("Title", "Название"),
            new ColumnVisibilityOption("Description", "Описание"),
            new ColumnVisibilityOption("Status", "Статус"),
            new ColumnVisibilityOption("Priority", "Приоритет"),
            new ColumnVisibilityOption("ProjectName", "Проект"),
            new ColumnVisibilityOption("AssigneeDisplayName", "Исполнитель"),
            new ColumnVisibilityOption("DueDateText", "Срок"),
            new ColumnVisibilityOption("CompletedAtText", "Факт"),
            new ColumnVisibilityOption("AuthorName", "Автор"),
            new ColumnVisibilityOption("CreatedAtText", "Создана"),
            new ColumnVisibilityOption("Actions", "Действия", isLocked: true)
        };

        public TasksViewModel(ITasksService tasksService, INotificationService notificationService, IPermissionService permissionService, IUsersService usersService, IDataRefreshService dataRefreshService)
        {
            _tasksService = tasksService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _usersService = usersService;
            _dataRefreshService = dataRefreshService;
            
            CanManageTasks = _permissionService.IsManagerOrAbove();
            CanCreateTasks = _permissionService.CanCreateTask();

            _dataRefreshService.Changed += OnDataRefresh;
        }

        private void OnDataRefresh(object? sender, DataRefreshScope scope)
        {
            if (!scope.HasFlag(DataRefreshScope.Tasks))
            {
                return;
            }

            System.Windows.Application.Current?.Dispatcher.BeginInvoke(() => _ = LoadAsync());
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            IsLoading = true;
            try
            {
                var items = await _tasksService.GetTasksAsync();
                Tasks = new ObservableCollection<TaskItem>(items);

                var users = await _usersService.GetUsersAsync();
                var filterUsers = users.Select(u => new UserItem
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Login = u.Login
                }).ToList();
                filterUsers.Insert(0, new UserItem { Id = -1, FirstName = "Все", LastName = "исполнители" });
                var previousAssigneeId = SelectedAssigneeFilterUser?.Id ?? -1;
                AssigneeFilterUsers = new ObservableCollection<UserItem>(filterUsers);
                SelectedAssigneeFilterUser = AssigneeFilterUsers.FirstOrDefault(u => u.Id == previousAssigneeId)
                    ?? AssigneeFilterUsers.First(u => u.Id == -1);

                CurrentPage = 1;
                ApplyFilterAndPaging();
            }
            catch (System.Exception ex)
            {
                _notificationService.ShowError($"Ошибка загрузки задач: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadAsync();
            _notificationService.ShowSuccess("Данные обновлены!");
        }

        [RelayCommand]
        private void Search()
        {
            CurrentPage = 1;
            ApplyFilterAndPaging();
        }

        [RelayCommand]
        private void FirstPage()
        {
            CurrentPage = 1;
            ApplyFilterAndPaging();
        }

        [RelayCommand]
        private void PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                ApplyFilterAndPaging();
            }
        }

        [RelayCommand]
        private void NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                ApplyFilterAndPaging();
            }
        }

        [RelayCommand]
        private void LastPage()
        {
            CurrentPage = TotalPages;
            ApplyFilterAndPaging();
        }

        public void SetSort(string column, ListSortDirection? requestedDirection)
        {
            if (string.IsNullOrWhiteSpace(column))
            {
                return;
            }

            if (_sortColumn == column)
            {
                _sortDirection = _sortDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            }
            else
            {
                _sortColumn = column;
                _sortDirection = requestedDirection ?? ListSortDirection.Ascending;
            }

            ApplyFilterAndPaging();
        }

        private void ApplyFilterAndPaging()
        {
            var filteredTasks = Tasks.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var term = SearchText.Trim();
                filteredTasks = filteredTasks.Where(t =>
                    SearchHelper.MatchesId(t.Id, term) ||
                    SearchHelper.MatchesId(t.ProjectId, term) ||
                    SearchHelper.MatchesText(t.Title, term) ||
                    SearchHelper.MatchesText(t.Description, term) ||
                    SearchHelper.MatchesText(t.ProjectName, term) ||
                    SearchHelper.MatchesText(t.AuthorName, term) ||
                    SearchHelper.MatchesText(t.AssigneeName, term));
            }

            if (SelectedStatusFilter >= 0)
            {
                filteredTasks = filteredTasks.Where(t => t.Status == SelectedStatusFilter);
            }

            if (SelectedPriorityFilter >= 0)
            {
                filteredTasks = filteredTasks.Where(t => t.Priority == SelectedPriorityFilter);
            }

            if (SelectedAssigneeFilterUser is { Id: > 0 } assignee)
            {
                filteredTasks = filteredTasks.Where(t =>
                    t.AssigneeId == assignee.Id ||
                    (t.AssigneeIds != null && t.AssigneeIds.Contains(assignee.Id)));
            }

            if (FilterDueFrom.HasValue)
            {
                var from = FilterDueFrom.Value.Date;
                filteredTasks = filteredTasks.Where(t => t.DueDate.HasValue && t.DueDate.Value.ToLocalTime().Date >= from);
            }

            if (FilterDueTo.HasValue)
            {
                var to = FilterDueTo.Value.Date;
                filteredTasks = filteredTasks.Where(t => t.DueDate.HasValue && t.DueDate.Value.ToLocalTime().Date <= to);
            }

            filteredTasks = ApplySort(filteredTasks);

            TotalItems = filteredTasks.Count();
            TotalPages = Math.Max(1, (int)System.Math.Ceiling((double)TotalItems / PageSize));

            if (CurrentPage > TotalPages && TotalPages > 0)
                CurrentPage = TotalPages;

            var pagedTasks = filteredTasks
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            PagedTasks = new ObservableCollection<TaskItem>(pagedTasks);
        }

        private IEnumerable<TaskItem> ApplySort(IEnumerable<TaskItem> source)
        {
            Func<TaskItem, object?> keySelector = _sortColumn switch
            {
                "Title" => t => t.Title,
                "StatusText" => t => t.Status,
                "PriorityText" => t => t.Priority,
                "ProjectName" => t => t.ProjectName,
                "AssigneeDisplayName" => t => t.AssigneeName,
                "DueDateText" => t => t.DueDate,
                "CompletedAtText" => t => t.CompletedAt,
                "AuthorName" => t => t.AuthorName,
                "CreatedAtText" => t => t.CreatedAt,
                _ => t => t.Id
            };

            return _sortDirection == ListSortDirection.Ascending
                ? source.OrderBy(keySelector)
                : source.OrderByDescending(keySelector);
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
            ApplyFilterAndPaging();
        }

        partial void OnPageSizeChanged(int value)
        {
            CurrentPage = 1;
            ApplyFilterAndPaging();
        }

        partial void OnCurrentPageChanged(int value)
        {
            var clamped = InputValidation.ClampPage(value, TotalPages);
            if (clamped != value)
            {
                CurrentPage = clamped;
                return;
            }

            ApplyFilterAndPaging();
        }

        partial void OnSelectedStatusFilterChanged(int value)
        {
            CurrentPage = 1;
            ApplyFilterAndPaging();
        }

        partial void OnSelectedPriorityFilterChanged(int value)
        {
            CurrentPage = 1;
            ApplyFilterAndPaging();
        }

        partial void OnSelectedAssigneeFilterUserChanged(UserItem? value)
        {
            CurrentPage = 1;
            ApplyFilterAndPaging();
        }

        partial void OnFilterDueFromChanged(DateTime? value)
        {
            CurrentPage = 1;
            ApplyFilterAndPaging();
        }

        partial void OnFilterDueToChanged(DateTime? value)
        {
            CurrentPage = 1;
            ApplyFilterAndPaging();
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SelectedStatusFilter = -1;
            SelectedPriorityFilter = -1;
            SelectedAssigneeFilterUser = AssigneeFilterUsers.FirstOrDefault(u => u.Id == -1);
            FilterDueFrom = null;
            FilterDueTo = null;
            SearchText = string.Empty;
            CurrentPage = 1;
            ApplyFilterAndPaging();
            _notificationService.ShowInfo("Фильтры сброшены");
        }

        [RelayCommand]
        private async Task CreateTaskAsync()
        {
            if (!_permissionService.CanCreateTask())
            {
                _notificationService.ShowWarning("У вас нет прав для создания задач");
                return;
            }

            try
            {
                var window = new Views.CreateEditTaskWindow
                {
                    Owner = System.Windows.Application.Current.MainWindow
                };

                await window.PrepareAsync();

                if (window.ShowDialog() == true)
                {
                    // Обновление через IDataRefreshService после сохранения
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task EditTaskAsync(TaskItem task)
        {
            if (task == null) return;

            if (!_permissionService.CanEditTask(task.AuthorId))
            {
                _notificationService.ShowWarning("У вас нет прав для редактирования этой задачи");
                return;
            }

            try
            {
                var window = new Views.CreateEditTaskWindow
                {
                    Owner = System.Windows.Application.Current.MainWindow
                };

                await window.PrepareAsync(task.Id);

                if (window.ShowDialog() == true)
                {
                    // Обновление через IDataRefreshService после сохранения
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка редактирования задачи: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task DeleteTaskAsync(TaskItem task)
        {
            if (task == null) return;

            if (!_permissionService.CanDeleteTask(task.AuthorId))
            {
                _notificationService.ShowWarning("У вас нет прав для удаления этой задачи");
                return;
            }

            var result = System.Windows.MessageBox.Show(
                $"Вы уверены, что хотите удалить задачу '{task.Title}'?",
                "Подтверждение удаления",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    await _tasksService.DeleteTaskAsync(task.Id);
                    _notificationService.ShowSuccess($"Задача '{task.Title}' успешно удалена");
                    _dataRefreshService.Notify(DataRefreshScopes.TaskRelated);
                }
                catch (Exception ex)
                {
                    _notificationService.ShowError($"Ошибка при удалении задачи: {ex.Message}");
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        public bool CanEditTaskItem(TaskItem task)
        {
            return _permissionService.CanEditTask(task.AuthorId);
        }

        public bool CanDeleteTaskItem(TaskItem task)
        {
            return _permissionService.CanDeleteTask(task.AuthorId);
        }

        [RelayCommand]
        private async Task ViewCommentsAsync(TaskItem task)
        {
            try
            {
                var viewModel = new TaskCommentsViewModel(
                    App.ServiceProvider.GetRequiredService<ICommentsService>(),
                    App.ServiceProvider.GetRequiredService<IAuthService>(),
                    App.ServiceProvider.GetRequiredService<INotificationService>(),
                    App.ServiceProvider.GetRequiredService<IPermissionService>(),
                    App.ServiceProvider.GetRequiredService<IProjectsService>(),
                    App.ServiceProvider.GetRequiredService<IDataRefreshService>());
                await viewModel.InitializeAsync(task.Id, task.Title, task.ProjectId);

                var window = new TaskCommentsWindow(viewModel)
                {
                    Owner = Application.Current.MainWindow
                };
                WindowFitHelper.ApplySizeBeforeShow(window, 820, 660);
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка открытия комментариев: {ex.Message}");
            }
        }
    }
}
