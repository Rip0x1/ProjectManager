using ProjectManagerApp.Models;
using ProjectManagementSystem.WPF.Services;
using System.Text.Json;

namespace ProjectManagerApp.Services
{
    public interface ICommentsService
    {
        Task<IList<CommentItem>> GetCommentsForTaskAsync(int taskId);
        Task<CommentItem?> CreateCommentAsync(CreateCommentDto commentDto);
        Task<bool> DeleteCommentAsync(int commentId);
    }

    public class CommentsService : ICommentsService
    {
        private readonly IApiClient _apiClient;

        public CommentsService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

    public async Task<IList<CommentItem>> GetCommentsForTaskAsync(int taskId)
    {
        try
        {
            var response = await _apiClient.GetAsync<List<CommentDto>>($"tasks/{taskId}/comments");
            if (response == null)
            {
                return new List<CommentItem>();
            }

            var comments = new List<CommentItem>();
            foreach (var commentDto in response)
            {
                var authorName = "Неизвестный пользователь";
                if (commentDto.CommentUserResponceDto != null)
                {
                    var firstName = commentDto.CommentUserResponceDto.FirstName ?? "";
                    var lastName = commentDto.CommentUserResponceDto.LastName ?? "";
                    
                    if (!string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName))
                    {
                        authorName = $"{firstName} {lastName}".Trim();
                    }
                    else if (!string.IsNullOrEmpty(commentDto.CommentUserResponceDto.Email))
                    {
                        authorName = commentDto.CommentUserResponceDto.Email;
                    }
                    else if (commentDto.AuthorId > 0)
                    {
                        authorName = $"Пользователь #{commentDto.AuthorId}";
                    }
                }
                else if (commentDto.AuthorId > 0)
                {
                    authorName = $"Пользователь #{commentDto.AuthorId}";
                }

                comments.Add(new CommentItem
                {
                    Id = commentDto.Id,
                    TaskId = commentDto.TaskId,
                    AuthorId = commentDto.AuthorId,
                    Content = commentDto.Content,
                    AuthorName = authorName,
                    CreatedAt = commentDto.CreatedAt
                });
            }

            return comments;
        }
        catch (Exception)
        {
            return new List<CommentItem>();
        }
    }

        public async Task<CommentItem?> CreateCommentAsync(CreateCommentDto commentDto)
        {
            try
            {
                var response = await _apiClient.PostAsync<CommentDto>($"tasks/{commentDto.TaskId}/comments", commentDto);
                if (response == null)
                {
                    return null;
                }

                var authorName = "Неизвестный пользователь";
                if (response.CommentUserResponceDto != null)
                {
                    var firstName = response.CommentUserResponceDto.FirstName ?? "";
                    var lastName = response.CommentUserResponceDto.LastName ?? "";
                    
                    if (!string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName))
                    {
                        authorName = $"{firstName} {lastName}".Trim();
                    }
                    else if (!string.IsNullOrEmpty(response.CommentUserResponceDto.Email))
                    {
                        authorName = response.CommentUserResponceDto.Email;
                    }
                    else if (response.AuthorId > 0)
                    {
                        authorName = $"Пользователь #{response.AuthorId}";
                    }
                }
                else if (response.AuthorId > 0)
                {
                    authorName = $"Пользователь #{response.AuthorId}";
                }

                return new CommentItem
                {
                    Id = response.Id,
                    TaskId = response.TaskId,
                    AuthorId = response.AuthorId,
                    Content = response.Content,
                    AuthorName = authorName,
                    CreatedAt = response.CreatedAt
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            try
            {
                await _apiClient.DeleteAsync($"comments/{commentId}");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
