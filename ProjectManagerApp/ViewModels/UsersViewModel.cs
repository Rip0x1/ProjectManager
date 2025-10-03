using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

        public bool CanManageUsers => App.ServiceProvider.GetRequiredService<IAuthService>().CurrentUserRole >= 2;

        public UsersViewModel(IUsersService usersService, INotificationService notificationService)
        {
            _usersService = usersService;
            _notificationService = notificationService;
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
            _notificationService.ShowInfo("Фильтры сброшены");
        }

        private void ApplySearchAndPagination()
        {
            var filteredUsers = Users.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filteredUsers = filteredUsers.Where(u =>
                    u.FirstName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    u.LastName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    u.RoleText.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
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
                var window = new CreateEditUserWindow(createEditUserViewModel);
                window.Owner = Application.Current.MainWindow;
                
                if (window.ShowDialog() == true)
                {
                    await LoadAsync();
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
                var window = new CreateEditUserWindow(createEditUserViewModel);
                window.Owner = Application.Current.MainWindow;
                
                if (window.ShowDialog() == true)
                {
                    await LoadAsync();
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
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить пользователя {user.FullName}?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await _usersService.DeleteUserAsync(user.Id);
                    await LoadAsync();
                    _notificationService.ShowSuccess("Пользователь удален успешно!");
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка удаления пользователя: {ex.Message}");
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
    }
}