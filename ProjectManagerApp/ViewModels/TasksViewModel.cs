using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace ProjectManagementSystem.WPF.ViewModels
{
    public partial class TasksViewModel : ObservableObject
    {
        private readonly ITasksService _tasksService;
        private readonly INotificationService _notificationService;

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


        public TasksViewModel(ITasksService tasksService, INotificationService notificationService)
        {
            _tasksService = tasksService;
            _notificationService = notificationService;
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
                _notificationService.ShowSuccess($"Показано {PagedTasks.Count} из {items.Count()} задач");
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
            Search();
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
    }
}