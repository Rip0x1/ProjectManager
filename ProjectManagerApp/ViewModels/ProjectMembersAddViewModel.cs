using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManagerApp.Models;
using ProjectManagerApp.Services;
using ProjectManagerApp.Views;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementSystem.WPF.Services;

namespace ProjectManagerApp.ViewModels
{
    public partial class ProjectMembersAddViewModel : ObservableObject
    {
        private readonly IProjectMembersService _projectMembersService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private string _projectTitle = string.Empty;

        [ObservableProperty]
        private ObservableCollection<AvailableUserItem> _availableUsers = new();

        [ObservableProperty]
        private bool _isLoading;

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

        private int _projectId;

        public ProjectMembersAddViewModel(IProjectMembersService projectMembersService, INotificationService notificationService)
        {
            _projectMembersService = projectMembersService;
            _notificationService = notificationService;
        }

        public async Task InitializeAsync(int projectId, string projectName)
        {
            _projectId = projectId;
            ProjectTitle = $"Добавить участников в: {projectName}";
            await LoadAvailableUsersAsync();
        }

        private async Task LoadAvailableUsersAsync()
        {
            try
            {
                IsLoading = true;
                var response = await _projectMembersService.GetAvailableUsersAsync(_projectId, SearchText, CurrentPage, 10);
                
                AvailableUsers.Clear();
                TotalCount = response.TotalCount;
                TotalPages = response.TotalPages;
                CurrentPage = response.CurrentPage;
                
                UpdatePaginationButtons();
                
                foreach (var user in response.Users)
                {
                    var userItem = new AvailableUserItem
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Role = user.Role
                    };
                    userItem.AddMemberCommand = new RelayCommand(async () => await AddMemberAsync(userItem));
                    AvailableUsers.Add(userItem);
                }
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

        private async Task AddMemberAsync(AvailableUserItem user)
        {
            try
            {
                await _projectMembersService.AddUserToProjectAsync(_projectId, user.UserId);
                _notificationService.ShowSuccess($"Пользователь {user.UserName} добавлен в проект");
                
                AvailableUsers.Remove(user);
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка добавления участника: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadAvailableUsersAsync();
            _notificationService.ShowInfo("Список пользователей обновлен");
        }

        [RelayCommand]
        private void Close()
        {
            Application.Current.Windows.OfType<ProjectMembersAddWindow>().FirstOrDefault()?.Close();
        }

        partial void OnSearchTextChanged(string value)
        {
            CurrentPage = 1;
            _ = LoadAvailableUsersAsync();
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            CurrentPage = 1;
            await LoadAvailableUsersAsync();
        }

        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadAvailableUsersAsync();
            }
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await LoadAvailableUsersAsync();
            }
        }

        [RelayCommand]
        private async Task FirstPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage = 1;
                await LoadAvailableUsersAsync();
            }
        }

        [RelayCommand]
        private async Task LastPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage = TotalPages;
                await LoadAvailableUsersAsync();
            }
        }

        private void UpdatePaginationButtons()
        {
            CanGoToPreviousPage = CurrentPage > 1;
            CanGoToNextPage = CurrentPage < TotalPages;
        }

        public bool HasNoAvailableUsers => !AvailableUsers.Any();
    }
}
