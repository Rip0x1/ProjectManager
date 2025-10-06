using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;
using ProjectManagerApp.Models;
using ProjectManagerApp.Services;
using ProjectManagerApp.ViewModels;
using ProjectManagerApp.Views;
using System.Collections.ObjectModel;
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

        [ObservableProperty]
        private ObservableCollection<TaskItem> _tasks = new ObservableCollection<TaskItem>();

        [ObservableProperty]
        private ObservableCollection<TaskItem> _pagedTasks = new ObservableCollection<TaskItem>();

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

        public TasksViewModel(ITasksService tasksService, INotificationService notificationService, IPermissionService permissionService)
        {
            _tasksService = tasksService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            
            CanManageTasks = _permissionService.IsManagerOrAbove();
            CanCreateTasks = _permissionService.CanCreateTask();
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            IsLoading = true;
            try
            {
                var items = await _tasksService.GetTasksAsync();
                Tasks = new ObservableCollection<TaskItem>(items);
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

        private void ApplyFilterAndPaging()
        {
            var filteredTasks = Tasks.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filteredTasks = filteredTasks.Where(t => 
                    t.Title.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase) ||
                    t.Description.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase) ||
                    t.ProjectName.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase) ||
                    t.AuthorName.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase) ||
                    t.AssigneeName.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase));
            }

            if (SelectedStatusFilter >= 0)
            {
                filteredTasks = filteredTasks.Where(t => t.Status == SelectedStatusFilter);
            }

            if (SelectedPriorityFilter >= 0)
            {
                filteredTasks = filteredTasks.Where(t => t.Priority == SelectedPriorityFilter);
            }

            TotalItems = filteredTasks.Count();
            TotalPages = (int)System.Math.Ceiling((double)TotalItems / PageSize);

            if (CurrentPage > TotalPages && TotalPages > 0)
                CurrentPage = TotalPages;

            var pagedTasks = filteredTasks
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            PagedTasks = new ObservableCollection<TaskItem>(pagedTasks);
        }

        partial void OnSearchTextChanged(string value)
        {
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

        [RelayCommand]
        private void ClearFilters()
        {
            SelectedStatusFilter = -1;
            SelectedPriorityFilter = -1;
            SearchText = string.Empty;
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
                var viewModel = App.GetService<CreateEditTaskViewModel>();
                var window = new Views.CreateEditTaskWindow();
                window.DataContext = viewModel;
                window.Owner = System.Windows.Application.Current.MainWindow;
                
                viewModel.CloseRequested += (s, success) =>
                {
                    window.DialogResult = success;
                    window.Close();
                };
                
                await viewModel.LoadAsync();
                
                if (window.ShowDialog() == true)
                {
                    await LoadAsync();
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
                var viewModel = App.GetService<CreateEditTaskViewModel>();
                var window = new Views.CreateEditTaskWindow();
                window.DataContext = viewModel;
                window.Owner = System.Windows.Application.Current.MainWindow;
                
                viewModel.CloseRequested += (s, success) =>
                {
                    window.DialogResult = success;
                    window.Close();
                };
                
                await viewModel.InitializeForEdit(task.Id);
                
                if (window.ShowDialog() == true)
                {
                    await LoadAsync();
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
                    await LoadAsync();
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
                var commentsService = App.ServiceProvider.GetRequiredService<ICommentsService>();
                var authService = App.ServiceProvider.GetRequiredService<IAuthService>();
                var notificationService = App.ServiceProvider.GetRequiredService<INotificationService>();

                var viewModel = new TaskCommentsViewModel(commentsService, authService, notificationService);
                await viewModel.InitializeAsync(task.Id, task.Title);

                var window = new TaskCommentsWindow(viewModel);
                window.Owner = Application.Current.MainWindow;
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка открытия комментариев: {ex.Message}");
            }
        }
    }
}