using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagerApp.Models;
using ProjectManagerApp.Services;
using ProjectManagerApp.ViewModels;
using ProjectManagerApp.Views;
using System.Windows;

namespace ProjectManagementSystem.WPF.ViewModels
{
    public partial class UsersViewModel : ObservableObject
    {
        private readonly IUsersService _usersService;
        private readonly INotificationService _notificationService;
        private readonly IDataRefreshService _dataRefreshService;

        [ObservableProperty]
        private ObservableCollection<UserItem> _users = new();

        [ObservableProperty]
        private ObservableCollection<UserItem> _pagedUsers = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private int _pageSize = 10;

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _totalPages = 1;

        [ObservableProperty]
        private int _totalUsers = 0;

        [ObservableProperty]
        private int _selectedRoleFilter = -1;

        public ObservableCollection<ColumnVisibilityOption> ColumnOptions { get; } = new()
        {
            new ColumnVisibilityOption("Id", "ID пользователя"),
            new ColumnVisibilityOption("FullName", "ФИО"),
            new ColumnVisibilityOption("Login", "Логин"),
            new ColumnVisibilityOption("Role", "Роль"),
            new ColumnVisibilityOption("CreatedAtText", "Создан"),
            new ColumnVisibilityOption("ManagedProjectsCount", "Проектов"),
            new ColumnVisibilityOption("AuthoredTasksCount", "Создал"),
            new ColumnVisibilityOption("AssignedTasksCount", "Назначено"),
            new ColumnVisibilityOption("CommentsCount", "Комментариев"),
            new ColumnVisibilityOption("Actions", "Действия", isLocked: true)
        };

        public bool CanManageUsers => App.ServiceProvider.GetRequiredService<IAuthService>().CurrentUserRole >= 2;

        public UsersViewModel(IUsersService usersService, INotificationService notificationService, IDataRefreshService dataRefreshService)
        {
            _usersService = usersService;
            _notificationService = notificationService;
            _dataRefreshService = dataRefreshService;

            _dataRefreshService.Changed += OnDataRefresh;
        }

        private void OnDataRefresh(object? sender, DataRefreshScope scope)
        {
            if (!scope.HasFlag(DataRefreshScope.Users))
            {
                return;
            }

            Application.Current?.Dispatcher.BeginInvoke(() => _ = LoadAsync());
        }

        [RelayCommand]
        public async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                var usersDto = await _usersService.GetUsersAsync();
                var users = usersDto.Select(UsersService.MapToUserItem).ToList();
                
                Users.Clear();
                foreach (var user in users)
                {
                    Users.Add(user);
                }

                TotalUsers = Users.Count;
                ApplySearchAndPagination();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка загрузки пользователей: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }


        [RelayCommand]
        private void Search()
        {
            ApplySearchAndPagination();
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
            ApplySearchAndPagination();
        }

        partial void OnPageSizeChanged(int value)
        {
            CurrentPage = 1;
            ApplySearchAndPagination();
        }

        partial void OnCurrentPageChanged(int value)
        {
            var clamped = InputValidation.ClampPage(value, TotalPages);
            if (clamped != value)
            {
                CurrentPage = clamped;
                return;
            }

            ApplySearchAndPagination();
        }

        partial void OnSelectedRoleFilterChanged(int value)
        {
            CurrentPage = 1;
            ApplySearchAndPagination();
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SelectedRoleFilter = -1;
            SearchText = string.Empty;
            CurrentPage = 1;
            ApplySearchAndPagination();
            _notificationService.ShowInfo("Фильтры сброшены");
        }

        private void ApplySearchAndPagination()
        {
            var filteredUsers = Users.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var term = SearchText.Trim();
                filteredUsers = filteredUsers.Where(u =>
                    SearchHelper.MatchesId(u.Id, term) ||
                    SearchHelper.MatchesText(u.FirstName, term) ||
                    SearchHelper.MatchesText(u.LastName, term) ||
                    SearchHelper.MatchesText(u.FullName, term) ||
                    SearchHelper.MatchesText(u.Login, term) ||
                    SearchHelper.MatchesText(u.RoleText, term));
            }

            if (SelectedRoleFilter >= 0)
            {
                filteredUsers = filteredUsers.Where(u => u.Role == SelectedRoleFilter);
            }

            var filteredList = filteredUsers.ToList();
            TotalPages = (int)Math.Ceiling((double)filteredList.Count / PageSize);

            if (CurrentPage > TotalPages && TotalPages > 0)
                CurrentPage = TotalPages;

            var pagedUsers = filteredList
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            PagedUsers.Clear();
            foreach (var user in pagedUsers)
            {
                PagedUsers.Add(user);
            }
        }

        [RelayCommand]
        private async Task CreateUserAsync()
        {
            try
            {
                var createEditUserViewModel = new CreateEditUserViewModel(_usersService, _notificationService);
                var window = new CreateEditUserWindow(createEditUserViewModel)
                {
                    Owner = Application.Current.MainWindow
                };
                WindowFitHelper.ApplySizeBeforeShow(window, 560, 580);

                if (window.ShowDialog() == true)
                {
                    _notificationService.ShowSuccess("Пользователь создан успешно!");
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка создания пользователя: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task EditUserAsync(UserItem user)
        {
            try
            {
                var createEditUserViewModel = new CreateEditUserViewModel(_usersService, _notificationService, user.Id);
                if (createEditUserViewModel.InitialLoadTask != null)
                {
                    await createEditUserViewModel.InitialLoadTask;
                }

                var window = new CreateEditUserWindow(createEditUserViewModel)
                {
                    Owner = Application.Current.MainWindow
                };
                WindowFitHelper.ApplySizeBeforeShow(window, 560, 580);

                if (window.ShowDialog() == true)
                {
                    _notificationService.ShowSuccess("Пользователь обновлен успешно!");
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка редактирования пользователя: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task DeleteUserAsync(UserItem user)
        {
            try
            {
                IsLoading = true;
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить пользователя {user.FullName}?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await _usersService.DeleteUserAsync(user.Id);
                    _notificationService.ShowSuccess($"Пользователь '{user.LastName}' удален успешно!");
                    _dataRefreshService.Notify(DataRefreshScopes.UserRelated);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка удаления пользователя: {ex.Message}");
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
        private void PreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                ApplySearchAndPagination();
            }
        }

        [RelayCommand]
        private void NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                ApplySearchAndPagination();
            }
        }

        [RelayCommand]
        private void FirstPage()
        {
            CurrentPage = 1;
            ApplySearchAndPagination();
        }

        [RelayCommand]
        private void LastPage()
        {
            CurrentPage = TotalPages;
            ApplySearchAndPagination();
        }

        [RelayCommand]
        private void ShowPermissionsMatrix()
        {
            PermissionMatrixHelper.Show();
        }
    }
}