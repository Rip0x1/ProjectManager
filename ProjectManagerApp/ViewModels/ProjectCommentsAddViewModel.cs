using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;
using ProjectManagerApp.Models;
using ProjectManagerApp.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace ProjectManagerApp.ViewModels
{
    public partial class ProjectCommentsAddViewModel : ObservableObject
    {
        private readonly INotificationService _notificationService;
        private readonly IApiClient _apiClient;
        private readonly IAuthService _authService;
        private readonly ICommentsService _commentsService;
        private readonly IDataRefreshService _dataRefreshService;
        private int _projectId;

        [ObservableProperty]
        private string _projectName = string.Empty;

        [ObservableProperty]
        private string _commentText = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;

        public ObservableCollection<PendingAttachment> PendingAttachments { get; } = new();

        public bool CanAddComment => (!string.IsNullOrWhiteSpace(CommentText) || PendingAttachments.Count > 0) &&
                                    (string.IsNullOrWhiteSpace(CommentText) || CommentText.Trim().Length <= ValidationLimits.CommentMax) &&
                                    !IsLoading;

        public ProjectCommentsAddViewModel(
            INotificationService notificationService,
            IApiClient apiClient,
            IAuthService authService,
            ICommentsService commentsService,
            IDataRefreshService dataRefreshService)
        {
            _notificationService = notificationService;
            _apiClient = apiClient;
            _authService = authService;
            _commentsService = commentsService;
            _dataRefreshService = dataRefreshService;
        }

        public async Task Initialize(int projectId, string projectName)
        {
            _projectId = projectId;
            ProjectName = projectName;
        }

        [RelayCommand]
        private void AddPendingAttachment()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Документы и изображения|*.doc;*.docx;*.pdf;*.xls;*.xlsx;*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.webp;*.txt|Все файлы|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                if (!AttachmentInputHelper.TryValidateFiles(dialog.FileNames, PendingAttachments.Count, out var error))
                {
                    _notificationService.ShowError(error!);
                    return;
                }

                foreach (var path in dialog.FileNames)
                {
                    PendingAttachments.Add(new PendingAttachment
                    {
                        FilePath = path,
                        FileName = System.IO.Path.GetFileName(path)
                    });
                }
                OnPropertyChanged(nameof(CanAddComment));
            }
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
        private async Task AddComment()
        {
            if (!ValidateInput())
            {
                return;
            }

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
                    Content = string.IsNullOrWhiteSpace(CommentText) ? " " : CommentText.Trim(),
                    AuthorId = currentUserId
                };

                var response = await _apiClient.PostAsync<CommentDto>($"comments/project/{_projectId}", dto);
                if (response != null)
                {
                    foreach (var pending in PendingAttachments.ToList())
                    {
                        await _commentsService.UploadCommentAttachmentAsync(response.Id, pending.FilePath, currentUserId);
                    }
                }

                _notificationService.ShowSuccess("Комментарий добавлен");
                _dataRefreshService.Notify(DataRefreshScopes.CommentRelated);
                CommentText = string.Empty;
                PendingAttachments.Clear();

                if (Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.DataContext == this) is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError(GetUserFriendlyErrorMessage(ex));
            }
            finally
            {
                IsLoading = false;
                OnPropertyChanged(nameof(CanAddComment));
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(CommentText) && PendingAttachments.Count == 0)
            {
                _notificationService.ShowError("Введите текст комментария или прикрепите файл");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(CommentText) && CommentText.Trim().Length > ValidationLimits.CommentMax)
            {
                _notificationService.ShowError($"Комментарий не должен превышать {ValidationLimits.CommentMax} символов");
                return false;
            }

            return true;
        }

        private static string GetUserFriendlyErrorMessage(Exception ex)
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

            return "Произошла ошибка при добавлении комментария. Проверьте введенные данные";
        }

        partial void OnCommentTextChanged(string value)
        {
            var sanitized = InputValidation.SanitizeMultilineText(value, ValidationLimits.CommentMax);
            if (sanitized != value)
            {
                CommentText = sanitized;
                return;
            }

            OnPropertyChanged(nameof(CanAddComment));
        }

        partial void OnIsLoadingChanged(bool value)
        {
            OnPropertyChanged(nameof(CanAddComment));
        }
    }
}
