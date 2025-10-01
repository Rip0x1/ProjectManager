using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;

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
        private async Task RefreshAsync()
        {
            await LoadAsync();
        }

        [RelayCommand]
        private void Search()
        {
            ApplySearchAndPagination();
        }

        [RelayCommand]
        private void FirstPage()
        {
            CurrentPage = 1;
            ApplySearchAndPagination();
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
        private void LastPage()
        {
            CurrentPage = TotalPages;
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
    }
}