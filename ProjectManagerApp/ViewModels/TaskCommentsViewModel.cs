using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ProjectManagerApp.Models;
using ProjectManagerApp.Services;
using ProjectManagerApp.Views;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Models;
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
        private readonly IPermissionService _permissionService;
        private readonly IProjectsService _projectsService;
        private readonly IDataRefreshService _dataRefreshService;
        private int _projectManagerId;

        [ObservableProperty]
        private string _taskTitle = string.Empty;

        [ObservableProperty]
        private int _taskId;

        [ObservableProperty]
        private string _newCommentContent = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        public ObservableCollection<CommentItem> Comments { get; } = new();
        public ObservableCollection<PendingAttachment> PendingAttachments { get; } = new();

        public bool CanAddComment =>
            (!string.IsNullOrWhiteSpace(NewCommentContent) || PendingAttachments.Count > 0) &&
            (string.IsNullOrWhiteSpace(NewCommentContent) || NewCommentContent.Trim().Length <= ValidationLimits.CommentMax) &&
            !IsLoading;

        public TaskCommentsViewModel(
            ICommentsService commentsService,
            IAuthService authService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IProjectsService projectsService,
            IDataRefreshService dataRefreshService)
        {
            _commentsService = commentsService;
            _authService = authService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _projectsService = projectsService;
            _dataRefreshService = dataRefreshService;
        }

        public async Task InitializeAsync(int taskId, string taskTitle, int? projectId = null)
        {
            TaskId = taskId;
            TaskTitle = taskTitle;
            _projectManagerId = 0;

            if (projectId.HasValue)
            {
                try
                {
                    var project = await _projectsService.GetProjectAsync(projectId.Value);
                    _projectManagerId = project.ManagerId;
                }
                catch
                {
                    _projectManagerId = 0;
                }
            }

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
                    CommentPermissionHelper.ApplyPermissions(comment, _permissionService, _projectManagerId);
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
        private void AddPendingAttachment()
        {
            PickAndAddFiles(PendingAttachments, () => OnPropertyChanged(nameof(CanAddComment)), _notificationService);
        }

        [RelayCommand]
        private void RemovePendingAttachment(PendingAttachment attachment)
        {
            if (attachment != null)
            {
                PendingAttachments.Remove(attachment);
                OnPropertyChanged(nameof(CanAddComment));
            }
        }

        [RelayCommand]
        private async Task AddAttachmentToCommentAsync(CommentItem? comment)
        {
            if (comment == null || !comment.CanAddAttachment)
            {
                return;
            }

            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Документы и изображения|*.doc;*.docx;*.pdf;*.xls;*.xlsx;*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.webp;*.txt|Все файлы|*.*"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            if (!AttachmentInputHelper.TryValidateFiles(dialog.FileNames, comment.Attachments.Count, out var error))
            {
                _notificationService.ShowError(error!);
                return;
            }

            try
            {
                IsLoading = true;
                foreach (var path in dialog.FileNames)
                {
                    var uploaded = await _commentsService.UploadCommentAttachmentAsync(
                        comment.Id, path, _authService.CurrentUserId);
                    uploaded.SourceCommentId = comment.Id;
                    uploaded.CanDelete = _permissionService.CanDeleteCommentAttachment(
                        comment.AuthorId, uploaded.UploadedById, _projectManagerId);
                    comment.Attachments.Add(uploaded);
                }

                _notificationService.ShowSuccess("Вложения добавлены");
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Не удалось прикрепить файл: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task AddCommentAsync()
        {
            if (string.IsNullOrWhiteSpace(NewCommentContent) && PendingAttachments.Count == 0)
            {
                _notificationService.ShowError("Введите текст комментария или прикрепите файл");
                return;
            }

            if (!string.IsNullOrWhiteSpace(NewCommentContent) && NewCommentContent.Trim().Length > ValidationLimits.CommentMax)
            {
                _notificationService.ShowError($"Комментарий не должен превышать {ValidationLimits.CommentMax} символов");
                return;
            }

            try
            {
                IsLoading = true;
                var commentDto = new CreateCommentDto
                {
                    Content = string.IsNullOrWhiteSpace(NewCommentContent) ? " " : NewCommentContent.Trim(),
                    TaskId = TaskId,
                    AuthorId = _authService.CurrentUserId
                };

                var newComment = await _commentsService.CreateCommentAsync(commentDto);
                if (newComment != null)
                {
                    foreach (var pending in PendingAttachments.ToList())
                    {
                        var uploaded = await _commentsService.UploadCommentAttachmentAsync(
                            newComment.Id, pending.FilePath, _authService.CurrentUserId);
                        uploaded.SourceCommentId = newComment.Id;
                        newComment.Attachments.Add(uploaded);
                    }

                    CommentPermissionHelper.ApplyPermissions(newComment, _permissionService, _projectManagerId);
                    Comments.Insert(0, newComment);
                    NewCommentContent = string.Empty;
                    PendingAttachments.Clear();
                    _notificationService.ShowSuccess("Комментарий добавлен");
                    _dataRefreshService.Notify(DataRefreshScopes.CommentRelated);
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
                OnPropertyChanged(nameof(CanAddComment));
            }
        }

        [RelayCommand]
        private async Task PreviewCommentAttachmentAsync(TaskAttachmentItem? attachment)
        {
            if (attachment?.SourceCommentId == null)
            {
                return;
            }

            try
            {
                IsLoading = true;
                var bytes = await _commentsService.DownloadCommentAttachmentAsync(attachment.SourceCommentId.Value, attachment.Id);
                var owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
                    ?? Application.Current.MainWindow;
                if (owner != null)
                {
                    AttachmentPreviewHelper.ShowPreview(owner, attachment.FileName, bytes, attachment.ContentType);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Не удалось открыть предпросмотр: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task DeleteCommentAttachmentAsync(TaskAttachmentItem? attachment)
        {
            if (attachment?.SourceCommentId == null || !attachment.CanDelete)
            {
                return;
            }

            try
            {
                IsLoading = true;
                await _commentsService.DeleteCommentAttachmentAsync(attachment.SourceCommentId.Value, attachment.Id);

                var comment = Comments.FirstOrDefault(c => c.Id == attachment.SourceCommentId);
                comment?.Attachments.Remove(attachment);
                _notificationService.ShowSuccess("Вложение удалено");
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Не удалось удалить вложение: {ex.Message}");
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
                        _dataRefreshService.Notify(DataRefreshScopes.CommentRelated);
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
            var sanitized = InputValidation.SanitizeMultilineText(value, ValidationLimits.CommentMax);
            if (sanitized != value)
            {
                NewCommentContent = sanitized;
                return;
            }

            OnPropertyChanged(nameof(CanAddComment));
        }

        partial void OnIsLoadingChanged(bool value)
        {
            OnPropertyChanged(nameof(CanAddComment));
        }

        private static void PickAndAddFiles(
            ObservableCollection<PendingAttachment> target,
            Action? onChanged,
            INotificationService notificationService)
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Документы и изображения|*.doc;*.docx;*.pdf;*.xls;*.xlsx;*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.webp;*.txt|Все файлы|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                if (!AttachmentInputHelper.TryValidateFiles(dialog.FileNames, target.Count, out var error))
                {
                    notificationService.ShowError(error!);
                    return;
                }

                foreach (var path in dialog.FileNames)
                {
                    target.Add(new PendingAttachment
                    {
                        FilePath = path,
                        FileName = System.IO.Path.GetFileName(path)
                    });
                }
                onChanged?.Invoke();
            }
        }
    }
}
