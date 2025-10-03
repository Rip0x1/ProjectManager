using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManagementSystem.WPF.Services;
using System.Windows;

namespace ProjectManagerApp.ViewModels
{
    public partial class ProjectCommentsAddViewModel : ObservableObject
    {
        private readonly INotificationService _notificationService;
        private readonly IApiClient _apiClient;
        private readonly IAuthService _authService;
        private int _projectId;

        [ObservableProperty]
        private string _projectName = string.Empty;

        [ObservableProperty]
        private string _commentText = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;

        public bool CanAddComment => !string.IsNullOrWhiteSpace(CommentText) && !IsLoading;

        public ProjectCommentsAddViewModel(INotificationService notificationService, IApiClient apiClient, IAuthService authService)
        {
            _notificationService = notificationService;
            _apiClient = apiClient;
            _authService = authService;
        }

        public async Task Initialize(int projectId, string projectName)
        {
            _projectId = projectId;
            ProjectName = projectName;
        }

        [RelayCommand]
        private async Task AddComment()
        {
            try
            {
                IsLoading = true;
                
                var currentUserId = _authService.CurrentUserId;
                if (currentUserId == 0)
                {
                    _notificationService.ShowError("Ошибка: пользователь не авторизован");
                    return;
                }
                
                var dto = new
                {
                    Content = CommentText,
                    AuthorId = currentUserId
                };
                
                await _apiClient.PostAsync<object>($"comments/project/{_projectId}", dto);
                
                _notificationService.ShowSuccess("Комментарий добавлен");
                CommentText = string.Empty;
                
                if (Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this) is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка добавления комментария: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnCommentTextChanged(string value)
        {
            OnPropertyChanged(nameof(CanAddComment));
        }
    }
}
