using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectManagementSystem.WPF.Services;
using ProjectManagerApp.Models;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Windows;

namespace ProjectManagerApp.ViewModels
{
    public partial class ProjectCommentsViewViewModel : ObservableObject
    {
        private readonly INotificationService _notificationService;
        private readonly IApiClient _apiClient;
        private readonly IAuthService _authService;
        private int _projectId;

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

        public ProjectCommentsViewViewModel(INotificationService notificationService, IApiClient apiClient, IAuthService authService)
        {
            _notificationService = notificationService;
            _apiClient = apiClient;
            _authService = authService;
        }

        public async Task Initialize(int projectId, string projectName)
        {
            _projectId = projectId;
            ProjectName = projectName;
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
                queryParams.Add($"pageSize=10");
                
                var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var response = await _apiClient.GetAsync<CommentsResponse>($"comments/project/{_projectId}{queryString}");
                
                Comments.Clear();
                if (response != null)
                {
                    foreach (var commentDto in response.Comments)
                    {
                        var comment = new CommentItem
                        {
                            Id = commentDto.Id,
                            Content = commentDto.Content,
                            CreatedAt = commentDto.CreatedAt,
                            TaskId = commentDto.TaskId,
                            AuthorId = commentDto.AuthorId,
                            AuthorName = $"{commentDto.CommentUserResponceDto.FirstName} {commentDto.CommentUserResponceDto.LastName}",
                            CanDelete = CanDeleteComment(commentDto.AuthorId)
                        };
                        Comments.Add(comment);
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
        private async Task AddComment()
        {
            try
            {
                var window = new ProjectManagerApp.Views.ProjectCommentsAddWindow();
                var viewModel = new ProjectCommentsAddViewModel(_notificationService, _apiClient, _authService);
                await viewModel.Initialize(_projectId, ProjectName);
                window.DataContext = viewModel;
                
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
            CurrentPage = 1;
            _ = Task.Run(async () =>
            {
                await Task.Delay(300); 
                await Application.Current.Dispatcher.InvokeAsync(async () => await LoadCommentsAsync());
            });
        }

        private bool CanDeleteComment(int authorId)
        {
            var currentUserId = _authService.CurrentUserId;
            var currentUserRole = _authService.CurrentUserRole;
            
            return currentUserRole == 2 || 
                   currentUserRole == 1 || 
                   authorId == currentUserId;
        }
    }
}
