using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace ProjectManagementSystem.WPF.ViewModels
{
    public partial class ProjectsViewModel : ObservableObject
    {
        private readonly IProjectsService _projectsService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private ObservableCollection<ProjectItem> _projects = new ObservableCollection<ProjectItem>();

        [ObservableProperty]
        private ObservableCollection<ProjectItem> _pagedProjects = new ObservableCollection<ProjectItem>();

        [ObservableProperty]
        private string _searchText = "";

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private int _pageSize = 10;

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _totalPages = 1;


        public ProjectsViewModel(IProjectsService projectsService, INotificationService notificationService)
        {
            _projectsService = projectsService;
            _notificationService = notificationService;
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            IsLoading = true;
            try
            {
                var items = await _projectsService.GetProjectsAsync();
                Projects = new ObservableCollection<ProjectItem>(items);
                CurrentPage = 1;
                ApplyFilterAndPaging();
                _notificationService.ShowSuccess($"Показано {PagedProjects.Count} из {items.Count} проектов");
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

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadAsync();
        }

        partial void OnSearchTextChanged(string value)
        {
            CurrentPage = 1;
            ApplyFilterAndPaging();
        }

        partial void OnPageSizeChanged(int value)
        {
            if (value <= 0) { PageSize = 10; return; }
            CurrentPage = 1;
            ApplyFilterAndPaging();
        }

        partial void OnCurrentPageChanged(int value)
        {
            if (value < 1) { CurrentPage = 1; return; }
            if (value > TotalPages) { CurrentPage = TotalPages; return; }
            ApplyFilterAndPaging();
        }

        [RelayCommand]
        private void NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
            }
        }

        [RelayCommand]
        private void PrevPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
            }
        }

        [RelayCommand]
        private void FirstPage()
        {
            CurrentPage = 1;
        }

        [RelayCommand]
        private void LastPage()
        {
            CurrentPage = TotalPages;
        }

        private void ApplyFilterAndPaging()
        {
            var query = Projects.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var term = SearchText.Trim().ToLowerInvariant();
                query = query.Where(p =>
                    (!string.IsNullOrEmpty(p.Name) && p.Name.ToLowerInvariant().Contains(term)) ||
                    (!string.IsNullOrEmpty(p.Description) && p.Description.ToLowerInvariant().Contains(term)) ||
                    (!string.IsNullOrEmpty(p.ManagerName) && p.ManagerName.ToLowerInvariant().Contains(term))
                );
            }

            var list = query.ToList();
            TotalPages = list.Count == 0 ? 1 : (int)System.Math.Ceiling(list.Count / (double)PageSize);
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;

            var pageItems = list.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
            PagedProjects = new ObservableCollection<ProjectItem>(pageItems);
        }
    }
}
