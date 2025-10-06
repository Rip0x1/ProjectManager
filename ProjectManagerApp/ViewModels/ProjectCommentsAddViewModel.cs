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

        public bool CanAddComment => !string.IsNullOrWhiteSpace(CommentText) && 
                                    CommentText.Trim().Length >= 1 && 
                                    CommentText.Trim().Length <= 1000 && 
                                    !IsLoading;

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
            if (!ValidateInput())
                return;

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
                    Content = CommentText.Trim(),
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
                var userFriendlyMessage = GetUserFriendlyErrorMessage(ex);
                _notificationService.ShowError(userFriendlyMessage);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool ValidateInput()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(CommentText))
            {
                errors.Add("Введите текст комментария");
            }
            else if (CommentText.Trim().Length < 1)
            {
                errors.Add("Комментарий не может быть пустым");
            }
            else if (CommentText.Trim().Length > 1000)
            {
                errors.Add("Комментарий не должен превышать 1000 символов");
            }

            if (errors.Any())
            {
                _notificationService.ShowError(string.Join("\n", errors));
                return false;
            }

            return true;
        }

        private string GetUserFriendlyErrorMessage(Exception ex)
        {
            var message = ex.Message.ToLower();

            if (message.Contains("содержимое") && message.Contains("обязательно"))
            {
                return "Содержимое комментария является обязательным полем";
            }

            if (message.Contains("проект") && message.Contains("не найден"))
            {
                return "Проект не найден";
            }

            if (message.Contains("пользователь") && message.Contains("не авторизован"))
            {
                return "Пользователь не авторизован";
            }

            if (message.Contains("некорректный запрос"))
            {
                return "Проверьте правильность введенных данных";
            }

            if (message.Contains("ошибка сервера"))
            {
                return "Временная ошибка сервера. Попробуйте позже";
            }

            if (message.Contains("недостаточно прав"))
            {
                return "У вас недостаточно прав для выполнения этой операции";
            }

            return "Произошла ошибка при добавлении комментария. Проверьте введенные данные";
        }

        partial void OnCommentTextChanged(string value)
        {
            OnPropertyChanged(nameof(CanAddComment));
        }
    }
}
