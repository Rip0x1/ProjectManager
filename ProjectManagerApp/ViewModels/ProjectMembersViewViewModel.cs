using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManagerApp.Models;
using ProjectManagerApp.Services;
using ProjectManagerApp.Views;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagementSystem.WPF.Services;
using ProjectManagerApp;

namespace ProjectManagerApp.ViewModels
{
    public partial class ProjectMembersViewViewModel : ObservableObject
    {
        private readonly IProjectMembersService _projectMembersService;
        private readonly INotificationService _notificationService;
        private int _projectId;

        [ObservableProperty]
        private string _projectTitle = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ProjectMemberItem> _projectMembers = new();

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

        [ObservableProperty]
        private double _scrollPosition = 0;

        public ProjectMembersViewViewModel(IProjectMembersService projectMembersService, INotificationService notificationService)
        {
            _projectMembersService = projectMembersService;
            _notificationService = notificationService;
        }

        public async Task InitializeAsync(int projectId, string projectName)
        {
            _projectId = projectId;
            ProjectTitle = $"Проект: {projectName}";
            await LoadProjectMembersAsync(projectId);
        }

        private async Task LoadProjectMembersAsync(int projectId)
        {
            try
            {
                IsLoading = true;
                var response = await _projectMembersService.GetProjectMembersAsync(projectId, SearchText, CurrentPage, 10);
                
                ProjectMembers.Clear();
                TotalCount = response.TotalCount;
                TotalPages = response.TotalPages;
                CurrentPage = response.CurrentPage;
                
                UpdatePaginationButtons();
                
                foreach (var member in response.Members)
                {
                    var memberItem = new ProjectMemberItem
                    {
                        Id = member.Id,
                        FirstName = member.FirstName,
                        LastName = member.LastName,
                        Email = member.Email,
                        Role = member.Role,
                        JoinedAt = member.JoinedAt
                    };
                    ProjectMembers.Add(memberItem);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка загрузки участников: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task AddMembersAsync()
        {
            try
            {
                var addWindow = new ProjectMembersAddWindow();
                var addViewModel = new ProjectMembersAddViewModel(_projectMembersService, _notificationService);
                await addViewModel.InitializeAsync(_projectId, ProjectTitle.Replace("Проект: ", ""));
                addWindow.DataContext = addViewModel;
                addWindow.ShowDialog();
                
                await LoadProjectMembersAsync(_projectId);
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка открытия окна добавления: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task RemoveMemberAsync(ProjectMemberItem member)
        {
            try
            {
                IsLoading = true;
                await _projectMembersService.RemoveUserFromProjectAsync(_projectId, member.Id);
                _notificationService.ShowSuccess($"Участник {member.FullName} удален из проекта.");
                await LoadProjectMembersAsync(_projectId);
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка удаления участника: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void Close()
        {
            Application.Current.Windows.OfType<ProjectMembersViewWindow>().FirstOrDefault()?.Close();
        }


        public bool HasNoMembers => !ProjectMembers.Any();

        partial void OnSearchTextChanged(string value)
        {
            CurrentPage = 1;
            _ = LoadProjectMembersAsync(_projectId);
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            CurrentPage = 1;
            await LoadProjectMembersAsync(_projectId);
        }

        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadProjectMembersAsync(_projectId);
            }
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await LoadProjectMembersAsync(_projectId);
            }
        }

        [RelayCommand]
        private async Task FirstPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage = 1;
                await LoadProjectMembersAsync(_projectId);
            }
        }

        [RelayCommand]
        private async Task LastPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage = TotalPages;
                await LoadProjectMembersAsync(_projectId);
            }
        }

        private void UpdatePaginationButtons()
        {
            CanGoToPreviousPage = CurrentPage > 1;
            CanGoToNextPage = CurrentPage < TotalPages;
        }
    }
}
