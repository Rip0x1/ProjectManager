using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManagerApp.Models;
using ProjectManagerApp.Services;
using ProjectManagerApp.Views;
using ProjectManagementSystem.WPF.Services;

namespace ProjectManagerApp.ViewModels
{
    public partial class ProjectMembersViewModel : ObservableObject
    {
        private readonly IProjectMembersService _projectMembersService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private string _windowTitle = "Участники проекта";

        [ObservableProperty]
        private int _projectId;

        [ObservableProperty]
        private string _projectName = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ProjectMemberItem> _projectMembers = new();

        [ObservableProperty]
        private ObservableCollection<AvailableUserItem> _availableUsers = new();

        [ObservableProperty]
        private bool _isLoading = false;

        public ProjectMembersViewModel(IProjectMembersService projectMembersService, INotificationService notificationService, int projectId, string projectName)
        {
            _projectMembersService = projectMembersService;
            _notificationService = notificationService;
            _projectId = projectId;
            _projectName = projectName;
            WindowTitle = $"Участники проекта: {projectName}";
            
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                await LoadProjectMembersAsync();
                await LoadAvailableUsersAsync();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка загрузки данных: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadProjectMembersAsync()
        {
            var response = await _projectMembersService.GetProjectMembersAsync(ProjectId);
            ProjectMembers.Clear();
            
            foreach (var member in response.Members)
            {
                ProjectMembers.Add(new ProjectMemberItem
                {
                    Id = member.Id,
                    FirstName = member.FirstName,
                    LastName = member.LastName,
                    Email = member.Email,
                    Role = member.Role,
                    JoinedAt = member.JoinedAt
                });
            }
        }

        private async Task LoadAvailableUsersAsync()
        {
            var response = await _projectMembersService.GetAvailableUsersAsync(ProjectId);
            AvailableUsers.Clear();
            
            foreach (var user in response.Users)
            {
                AvailableUsers.Add(new AvailableUserItem
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role
                });
            }
        }

        [RelayCommand]
        private async Task AddMemberAsync(AvailableUserItem user)
        {
            try
            {
                IsLoading = true;
                await _projectMembersService.AddUserToProjectAsync(ProjectId, user.Id);
                await LoadDataAsync();
                _notificationService.ShowSuccess($"Пользователь {user.FullName} добавлен в проект");
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка добавления пользователя: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RemoveMemberAsync(ProjectMemberItem member)
        {
            try
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить {member.FullName} из проекта?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsLoading = true;
                    await _projectMembersService.RemoveUserFromProjectAsync(ProjectId, member.Id);
                    await LoadDataAsync();
                    _notificationService.ShowSuccess($"Пользователь {member.FullName} удален из проекта");
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
    }
}
