using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectManagementSystem.WPF.ViewModels
{
    public partial class ProjectsViewModel : ObservableObject
    {
        private readonly IProjectsService _projectsService;
        private readonly INotificationService _notificationService;
        private readonly IUsersService _usersService;
        private readonly IPermissionService _permissionService;

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


        [ObservableProperty]
        private bool _canCreateProjects = false;

        [ObservableProperty]
        private bool _canManageProjects = false;

        [ObservableProperty]
        private int _selectedStatusFilter = -1; 

        public ProjectsViewModel(IProjectsService projectsService, INotificationService notificationService, IUsersService usersServices, IPermissionService permissionService)
        {
            _projectsService = projectsService;
            _notificationService = notificationService;
            _usersService = usersServices;
            _permissionService = permissionService;
            
            CanCreateProjects = _permissionService.CanCreateProject();
            CanManageProjects = _permissionService.IsManagerOrAbove();
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
            _notificationService.ShowSuccess("Данные обновлены!");
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

        partial void OnSelectedStatusFilterChanged(int value)
        {
            CurrentPage = 1;
            ApplyFilterAndPaging();
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SelectedStatusFilter = -1;
            SearchText = string.Empty;
            _notificationService.ShowInfo("Фильтры сброшены");
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

            if (SelectedStatusFilter >= 0)
            {
                query = query.Where(p => p.Status == SelectedStatusFilter);
            }

            var list = query.ToList();
            TotalPages = list.Count == 0 ? 1 : (int)System.Math.Ceiling(list.Count / (double)PageSize);
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;

            var pageItems = list.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
            PagedProjects = new ObservableCollection<ProjectItem>(pageItems);
        }

        [RelayCommand]
        private async Task CreateProject()
        {
            if (!_permissionService.CanCreateProject())
            {
                _notificationService.ShowWarning("У вас нет прав для создания проектов");
                return;
            }

            try
            {
                var viewModel = new CreateEditProjectViewModel(_projectsService, _usersService, _notificationService);
                var window = new Views.CreateEditProjectWindow(viewModel);
                window.Owner = System.Windows.Application.Current.MainWindow;
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
        private void EditProject(ProjectItem project)
        {
            if (project == null) return;

            if (!_permissionService.CanEditProject(project.ManagerId))
            {
                _notificationService.ShowWarning("У вас нет прав для редактирования этого проекта");
                return;
            }

            var viewModel = new CreateEditProjectViewModel(_projectsService, _usersService, _notificationService, project.Id);
            var window = new Views.CreateEditProjectWindow(viewModel);
            
            if (window.ShowDialog() == true)
            {
                _ = LoadAsync();
            }
        }

        [RelayCommand]
        private async Task DeleteProjectAsync(ProjectItem project)
        {
            if (project == null) return;

            if (!_permissionService.CanDeleteProject(project.ManagerId))
            {
                _notificationService.ShowWarning("У вас нет прав для удаления этого проекта");
                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить проект \"{project.Name}\"?\n\nЭто действие нельзя отменить!",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                await _projectsService.DeleteProjectAsync(project.Id);
                _notificationService.ShowSuccess($"Проект \"{project.Name}\" успешно удален");
                await LoadAsync();
            }
            catch (System.Exception ex)
            {
                _notificationService.ShowError($"Ошибка удаления проекта: {ex.Message}");
            }
        }
        
        public bool CanEditProjectItem(ProjectItem project)
        {
            return _permissionService.CanEditProject(project.ManagerId);
        }
        
        public bool CanDeleteProjectItem(ProjectItem project)
        {
            return _permissionService.CanDeleteProject(project.ManagerId);
        }

        [RelayCommand]
        private async Task ManageMembersAsync(ProjectItem project)
        {
            try
            {
                var window = new ProjectManagerApp.Views.ProjectMembersViewWindow();
                var viewModel = App.ServiceProvider.GetService<ProjectManagerApp.ViewModels.ProjectMembersViewViewModel>();
                if (viewModel != null)
                {
                    await viewModel.InitializeAsync(project.Id, project.Name);
                    window.DataContext = viewModel;
                    window.Owner = Application.Current.MainWindow;
                    window.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка управления участниками: {ex.Message}");
            }
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
        private void FirstPage()
        {
            CurrentPage = 1;
            ApplyFilterAndPaging();
        }

        [RelayCommand]
        private void LastPage()
        {
            CurrentPage = TotalPages;
            ApplyFilterAndPaging();
        }
    }
}
