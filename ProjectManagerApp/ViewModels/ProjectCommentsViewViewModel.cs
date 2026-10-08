using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Services;
using ProjectManagerApp.Models;
using ProjectManagerApp.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace ProjectManagerApp.ViewModels
{
    public partial class ProjectCommentsViewViewModel : ObservableObject
    {
        private readonly INotificationService _notificationService;
        private readonly IApiClient _apiClient;
        private readonly IAuthService _authService;
        private readonly ICommentsService _commentsService;
        private readonly IPermissionService _permissionService;
        private readonly IDataRefreshService _dataRefreshService;
        private int _projectId;
        private int _projectManagerId;

        [ObservableProperty]
        private string _projectName = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private ObservableCollection<CommentItem> _comments = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _totalPages = 1;

        [ObservableProperty]
        private int _totalCount = 0;

        [ObservableProperty]
        private bool _canGoToPreviousPage = false;

        [ObservableProperty]
        private bool _canGoToNextPage = false;

        public ProjectCommentsViewViewModel(
            INotificationService notificationService,
            IApiClient apiClient,
            IAuthService authService,
            ICommentsService commentsService,
            IPermissionService permissionService,
            IDataRefreshService dataRefreshService)
        {
            _notificationService = notificationService;
            _apiClient = apiClient;
            _authService = authService;
            _commentsService = commentsService;
            _permissionService = permissionService;
            _dataRefreshService = dataRefreshService;
        }

        public async Task Initialize(int projectId, string projectName, int projectManagerId)
        {
            _projectId = projectId;
            ProjectName = projectName;
            _projectManagerId = projectManagerId;
            await LoadCommentsAsync();
        }

        [RelayCommand]
        private async Task LoadCommentsAsync()
        {
            try
            {
                IsLoading = true;

                var queryParams = new List<string>();
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    queryParams.Add($"search={Uri.EscapeDataString(SearchText)}");
                }
                queryParams.Add($"page={CurrentPage}");
                queryParams.Add("pageSize=10");

                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var response = await _apiClient.GetAsync<CommentsResponse>($"comments/project/{_projectId}{queryString}");

                Comments.Clear();
                if (response != null)
                {
                    foreach (var commentDto in response.Comments)
                    {
                        Comments.Add(CommentPermissionHelper.MapCommentDto(commentDto, _permissionService, _projectManagerId));
                    }

                    TotalCount = response.TotalCount;
                    TotalPages = response.TotalPages;
                    CurrentPage = response.CurrentPage;
                }

                UpdatePaginationButtons();
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
        private async Task DeleteComment(CommentItem comment)
        {
            try
            {
                IsLoading = true;
                await _apiClient.DeleteAsync($"comments/{comment.Id}");
                _notificationService.ShowSuccess("Комментарий удален");
                _dataRefreshService.Notify(DataRefreshScopes.CommentRelated);
                await LoadCommentsAsync();
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
        private async Task PreviewCommentAttachmentAsync(ProjectManagementSystem.WPF.Models.TaskAttachmentItem? attachment)
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
        private async Task DeleteCommentAttachmentAsync(ProjectManagementSystem.WPF.Models.TaskAttachmentItem? attachment)
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
        private async Task AddComment()
        {
            try
            {
                var window = new ProjectManagerApp.Views.ProjectCommentsAddWindow();
                var viewModel = new ProjectCommentsAddViewModel(
                    _notificationService, _apiClient, _authService, _commentsService, _dataRefreshService);
                await viewModel.Initialize(_projectId, ProjectName);
                window.DataContext = viewModel;
                WindowFitHelper.ApplySizeBeforeShow(window, 540, 520);

                if (window.ShowDialog() == true)
                {
                    await LoadCommentsAsync();
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Ошибка открытия окна добавления комментария: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            CurrentPage = 1;
            await LoadCommentsAsync();
        }

        [RelayCommand]
        private async Task FirstPageAsync()
        {
            CurrentPage = 1;
            await LoadCommentsAsync();
        }

        [RelayCommand]
        private async Task PreviousPageAsync()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                await LoadCommentsAsync();
            }
        }

        [RelayCommand]
        private async Task NextPageAsync()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                await LoadCommentsAsync();
            }
        }

        [RelayCommand]
        private async Task LastPageAsync()
        {
            CurrentPage = TotalPages;
            await LoadCommentsAsync();
        }

        private void UpdatePaginationButtons()
        {
            CanGoToPreviousPage = CurrentPage > 1;
            CanGoToNextPage = CurrentPage < TotalPages;
        }

        partial void OnSearchTextChanged(string value)
        {
            var sanitized = InputValidation.SanitizeSearch(value);
            if (sanitized != value)
            {
                SearchText = sanitized;
                return;
            }

            CurrentPage = 1;
            _ = Task.Run(async () =>
            {
                await Task.Delay(300);
                await Application.Current.Dispatcher.InvokeAsync(async () => await LoadCommentsAsync());
            });
        }
    }
}
