using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManagerApp.Models;
using ProjectManagerApp.Services;
using ProjectManagementSystem.WPF.Services;
using ProjectManagementSystem.WPF.Models;
using System.Windows;

namespace ProjectManagerApp.ViewModels
{
    public partial class ProjectTasksViewModel : ObservableObject
    {
        private readonly ITasksService _tasksService;
        private readonly IProjectsService _projectsService;
        private readonly IUsersService _usersService;
        private readonly IAuthService _authService;
        private readonly INotificationService _notificationService;
        private readonly IApiClient _apiClient;
        private int _projectId;

        [ObservableProperty]
        private string _projectName = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private ObservableCollection<TaskItem> _tasks = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _totalPages = 1;

        [ObservableProperty]
        private int _totalCount = 0;

        [ObservableProperty]
        private bool _canGoToPreviousPage = false;

        [ObservableProperty]
        private bool _canGoToNextPage = false;

        private const int PageSize = 10;

        public ProjectTasksViewModel(
            ITasksService tasksService,
            IProjectsService projectsService,
            IUsersService usersService,
            IAuthService authService,
            INotificationService notificationService,
            IApiClient apiClient)
        {
            _tasksService = tasksService;
            _projectsService = projectsService;
            _usersService = usersService;
            _authService = authService;
            _notificationService = notificationService;
            _apiClient = apiClient;
        }

        public async Task Initialize(int projectId, string projectName)
        {
            _projectId = projectId;
            ProjectName = projectName;
            await LoadTasksAsync();
        }

        [RelayCommand]
        private async Task LoadTasksAsync()
        {
            try
            {
                IsLoading = true;
                
                var queryParams = new List<string>();
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    queryParams.Add($"search={Uri.EscapeDataString(SearchText)}");
                }
                queryParams.Add($"page={CurrentPage}");
                queryParams.Add($"pageSize={PageSize}");
                queryParams.Add($"projectId={_projectId}");
                
                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var response = await _apiClient.GetAsync<ApiTasksResponse>($"tasks{queryString}");
                
                Tasks.Clear();
                if (response != null)
                {
                    foreach (var apiTask in response.Tasks)
                    {
                        var taskItem = new TaskItem
                        {
                            Id = apiTask.Id,
                            Title = apiTask.Title,
                            Description = apiTask.Description,
                            Status = apiTask.Status,
                            Priority = apiTask.Priority,
                            ProjectId = apiTask.ProjectId,
                            ProjectName = apiTask.ProjectName,
                            AuthorId = apiTask.AuthorId,
                            AuthorName = !string.IsNullOrEmpty(apiTask.AuthorName) ? apiTask.AuthorName : $"Пользователь #{apiTask.AuthorId}",
                            AssigneeId = apiTask.AssigneeId,
                            AssigneeName = apiTask.AssigneeName,
                            CreatedAt = apiTask.CreatedAt,
                            UpdatedAt = apiTask.UpdatedAt,
                            PlannedHours = apiTask.PlannedHours,
                            ActualHours = apiTask.ActualHours,
                            CommentsCount = apiTask.CommentsCount
                        };
                        Tasks.Add(taskItem);
                    }
                    
                    TotalCount = response.TotalCount;
                    TotalPages = response.TotalPages;
                    CurrentPage = response.CurrentPage;
                }
                
                UpdatePaginationButtons();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка загрузки задач: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task AddTaskAsync()
        {
            try
            {
                var window = new ProjectManagementSystem.WPF.Views.CreateEditTaskWindow(null, _projectId, ProjectName);
                
                if (window.ShowDialog() == true)
                {
                    await LoadTasksAsync();
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка открытия создания задачи: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task EditTaskAsync(TaskItem task)
        {
            try
            {
                var window = new ProjectManagementSystem.WPF.Views.CreateEditTaskWindow(task.Id);
                
                if (window.ShowDialog() == true)
                {
                    await LoadTasksAsync();
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка открытия редактирования задачи: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task DeleteTaskAsync(TaskItem task)
        {
            try
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить задачу '{task.Title}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await _tasksService.DeleteTaskAsync(task.Id);
                    _notificationService.ShowSuccess("Задача удалена");
                    
                    Tasks.Remove(task);
                    TotalCount = Math.Max(0, TotalCount - 1);
                    UpdatePaginationButtons();
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка удаления задачи: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadTasksAsync();
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            CurrentPage = 1;
            await LoadTasksAsync();
        }

        [RelayCommand]
        private async Task FirstPageAsync()
        {
            CurrentPage = 1;
            await LoadTasksAsync();
        }

        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadTasksAsync();
            }
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await LoadTasksAsync();
            }
        }

        [RelayCommand]
        private async Task LastPageAsync()
        {
            CurrentPage = TotalPages;
            await LoadTasksAsync();
        }

        private void UpdatePaginationButtons()
        {
            CanGoToPreviousPage = CurrentPage > 1;
            CanGoToNextPage = CurrentPage < TotalPages;
        }

        partial void OnSearchTextChanged(string value)
        {
            CurrentPage = 1;
            _ = LoadTasksAsync();
        }
    }
}
