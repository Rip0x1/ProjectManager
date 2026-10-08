using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;
using ProjectManagementSystem.WPF.Views;
using System.Windows;

namespace ProjectManagementSystem.WPF.ViewModels
{
    public partial class UserDetailsViewModel : ObservableObject
    {
        private readonly IUsersService _usersService;
        private readonly INotificationService _notificationService;
        private readonly IDataRefreshService _dataRefreshService;
        private int _userId;

        [ObservableProperty]
        private UserItem _currentUser = new();

        public UserDetailsViewModel(
            IUsersService usersService,
            INotificationService notificationService,
            IDataRefreshService dataRefreshService)
        {
            _usersService = usersService;
            _notificationService = notificationService;
            _dataRefreshService = dataRefreshService;
            _dataRefreshService.Changed += OnDataRefresh;
        }

        public async Task InitializeAsync(UserItem user)
        {
            _userId = user.Id;
            CurrentUser = user;
            await ReloadUserAsync();
        }

        private void OnDataRefresh(object? sender, DataRefreshScope scope)
        {
            if (!scope.HasFlag(DataRefreshScope.Users))
            {
                return;
            }

            Application.Current?.Dispatcher.BeginInvoke(async () =>
            {
                await ReloadUserAsync();
            });
        }

        private async Task ReloadUserAsync()
        {
            if (_userId <= 0)
            {
                return;
            }

            try
            {
                var dto = await _usersService.GetUserAsync(_userId);
                if (dto != null)
                {
                    CurrentUser = UsersService.MapToUserItem(dto);
                }
            }
            catch
            {
            }
        }

        [RelayCommand]
        private Task ViewProjectsAsync() => OpenRelatedAsync(UserRelatedKind.Projects);

        [RelayCommand]
        private Task ViewAuthoredTasksAsync() => OpenRelatedAsync(UserRelatedKind.AuthoredTasks);

        [RelayCommand]
        private Task ViewAssignedTasksAsync() => OpenRelatedAsync(UserRelatedKind.AssignedTasks);

        [RelayCommand]
        private Task ViewCommentsAsync() => OpenRelatedAsync(UserRelatedKind.Comments);

        private async Task OpenRelatedAsync(UserRelatedKind kind)
        {
            try
            {
                var window = new UserRelatedListWindow();
                var viewModel = App.GetService<UserRelatedListViewModel>();
                await viewModel.InitializeAsync(kind, CurrentUser.Id, CurrentUser.FullName);
                window.DataContext = viewModel;
                window.Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                    ?? Application.Current.MainWindow;
                WindowFitHelper.ApplySizeBeforeShow(window, 860, 640);
                window.ShowDialog();
                await ReloadUserAsync();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Не удалось открыть список: {ex.Message}");
            }
        }
    }
}
