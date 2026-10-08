using ProjectManagerApp.Models;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ProjectManagerApp.Services
{
    public interface ICommentsService
    {
        Task<IList<CommentItem>> GetCommentsForTaskAsync(int taskId);
        Task<CommentItem?> CreateCommentAsync(CreateCommentDto commentDto);
        Task<bool> DeleteCommentAsync(int commentId);
        Task<TaskAttachmentItem> UploadCommentAttachmentAsync(int commentId, string filePath, int? uploadedById);
        Task<byte[]> DownloadCommentAttachmentAsync(int commentId, int attachmentId);
        Task DeleteCommentAttachmentAsync(int commentId, int attachmentId);
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
                    else if (!string.IsNullOrEmpty(commentDto.CommentUserResponceDto.Login))
                    {
                        authorName = commentDto.CommentUserResponceDto.Login;
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
                    CreatedAt = commentDto.CreatedAt,
                    Attachments = new System.Collections.ObjectModel.ObservableCollection<TaskAttachmentItem>(
                        (commentDto.Attachments ?? new List<CommentAttachmentDto>()).Select(a => MapAttachment(a, commentDto.Id)))
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
                    else if (!string.IsNullOrEmpty(response.CommentUserResponceDto.Login))
                    {
                        authorName = response.CommentUserResponceDto.Login;
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
                    CreatedAt = response.CreatedAt,
                    Attachments = new System.Collections.ObjectModel.ObservableCollection<TaskAttachmentItem>(
                        (response.Attachments ?? new List<CommentAttachmentDto>()).Select(a => MapAttachment(a, response.Id)))
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

        public async Task<TaskAttachmentItem> UploadCommentAttachmentAsync(int commentId, string filePath, int? uploadedById)
        {
            await using var stream = File.OpenRead(filePath);
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(stream);
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(extension));
            content.Add(fileContent, "file", Path.GetFileName(filePath));

            var query = uploadedById.HasValue ? $"?uploadedById={uploadedById.Value}" : string.Empty;
            var response = await _apiClient.PostMultipartAsync<CommentAttachmentDto>($"comments/{commentId}/attachments{query}", content);
            return MapAttachment(response, commentId);
        }

        public async Task<byte[]> DownloadCommentAttachmentAsync(int commentId, int attachmentId)
        {
            return await _apiClient.GetBytesAsync($"comments/{commentId}/attachments/{attachmentId}");
        }

        public async Task DeleteCommentAttachmentAsync(int commentId, int attachmentId)
        {
            await _apiClient.DeleteAsync($"comments/{commentId}/attachments/{attachmentId}");
        }

        private static TaskAttachmentItem MapAttachment(CommentAttachmentDto item, int commentId)
        {
            return new TaskAttachmentItem
            {
                Id = item.Id,
                FileName = item.FileName,
                ContentType = item.ContentType,
                FileSize = item.FileSize,
                UploadedAt = item.UploadedAt,
                UploadedById = item.UploadedById,
                SourceCommentId = commentId
            };
        }

        private static string GetContentType(string extension) => extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}
