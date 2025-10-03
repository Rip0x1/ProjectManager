using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagerApp.Models;
using ProjectManagerApp.Services;
using ProjectManagerApp.Views;
using ProjectManagementSystem.WPF.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace ProjectManagerApp.ViewModels
{
    public partial class TaskCommentsViewModel : ObservableObject
    {
        private readonly ICommentsService _commentsService;
        private readonly IAuthService _authService;
        private readonly INotificationService _notificationService;

        [ObservableProperty]
        private string _taskTitle = string.Empty;

        [ObservableProperty]
        private int _taskId;

        [ObservableProperty]
        private string _newCommentContent = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        public ObservableCollection<CommentItem> Comments { get; } = new();

        public bool CanAddComment => !string.IsNullOrWhiteSpace(NewCommentContent) && !IsLoading;

        public TaskCommentsViewModel(ICommentsService commentsService, IAuthService authService, INotificationService notificationService)
        {
            _commentsService = commentsService;
            _authService = authService;
            _notificationService = notificationService;
        }

        public async Task InitializeAsync(int taskId, string taskTitle)
        {
            TaskId = taskId;
            TaskTitle = taskTitle;
            await LoadCommentsAsync();
        }

        [RelayCommand]
        private async Task LoadCommentsAsync()
        {
            try
            {
                IsLoading = true;
                var comments = await _commentsService.GetCommentsForTaskAsync(TaskId);
                
                Comments.Clear();
                foreach (var comment in comments)
                {
                    var currentUserRole = _authService.CurrentUserRole;
                    var isAdminOrManager = currentUserRole >= 1;
                    var isOwnComment = comment.AuthorId == _authService.CurrentUser?.Id;
                    
                    comment.CanDelete = isAdminOrManager || isOwnComment;
                    Comments.Add(comment);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка загрузки комментариев: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task AddCommentAsync()
        {
            if (string.IsNullOrWhiteSpace(NewCommentContent))
            {
                _notificationService.ShowError("Введите текст комментария");
                return;
            }

            try
            {
                IsLoading = true;
                var commentDto = new CreateCommentDto
                {
                    Content = NewCommentContent.Trim(),
                    TaskId = TaskId,
                    AuthorId = _authService.CurrentUserId
                };

                var newComment = await _commentsService.CreateCommentAsync(commentDto);
                if (newComment != null)
                {
                    var currentUserRole = _authService.CurrentUserRole;
                    var isAdminOrManager = currentUserRole >= 1;
                    var isOwnComment = newComment.AuthorId == _authService.CurrentUserId;
                    
                    newComment.CanDelete = isAdminOrManager || isOwnComment;
                    Comments.Add(newComment);
                    NewCommentContent = string.Empty;
                    _notificationService.ShowSuccess("Комментарий добавлен");
                }
                else
                {
                    _notificationService.ShowError("Ошибка добавления комментария");
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

        [RelayCommand]
        private async Task DeleteCommentAsync(int commentId)
        {
            try
            {
                var result = MessageBox.Show(
                    "Вы уверены, что хотите удалить этот комментарий?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    IsLoading = true;
                    var success = await _commentsService.DeleteCommentAsync(commentId);
                    if (success)
                    {
                        var commentToRemove = Comments.FirstOrDefault(c => c.Id == commentId);
                        if (commentToRemove != null)
                        {
                            Comments.Remove(commentToRemove);
                        }
                        _notificationService.ShowSuccess("Комментарий удален");
                    }
                    else
                    {
                        _notificationService.ShowError("Ошибка удаления комментария");
                    }
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка удаления комментария: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            if (Application.Current.Windows.OfType<TaskCommentsWindow>().FirstOrDefault() is { } window)
            {
                window.Close();
            }
        }

        partial void OnNewCommentContentChanged(string value)
        {
            OnPropertyChanged(nameof(CanAddComment));
        }

        partial void OnIsLoadingChanged(bool value)
        {
            OnPropertyChanged(nameof(CanAddComment));
        }
    }
}
