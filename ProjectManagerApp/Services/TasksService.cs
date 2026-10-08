using ProjectManagementSystem.WPF.Models;
using ProjectManagerApp.Models;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ProjectManagementSystem.WPF.Services
{
    public class TasksService : ITasksService
    {
        private readonly IApiClient _apiClient;

        public TasksService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IEnumerable<TaskItem>> GetTasksAsync()
        {
            try
            {
                var response = await _apiClient.GetAsync<ApiTasksResponse>("tasks?page=1&pageSize=1000");
                return response.Tasks.Select(MapToTaskItem);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки задач: {ex.Message}", ex);
            }
        }

        public async Task<TaskDto> GetTaskAsync(int id)
        {
            var response = await _apiClient.GetAsync<ApiTaskResponseDto>($"tasks/{id}");
            return MapToTaskDto(response);
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByStatusAsync(int status)
        {
            var allTasks = await GetTasksAsync();
            return allTasks.Where(t => t.Status == status);
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByPriorityAsync(int priority)
        {
            var allTasks = await GetTasksAsync();
            return allTasks.Where(t => t.Priority == priority);
        }

        public async Task<TaskDto> CreateTaskAsync(CreateUpdateTaskDto task)
        {
            return await _apiClient.PostAsync<TaskDto>("tasks", task);
        }

        public async Task UpdateTaskAsync(int id, CreateUpdateTaskDto task)
        {
            await _apiClient.PutAsync<TaskDto>($"tasks/{id}", task);
        }

        public async Task DeleteTaskAsync(int id)
        {
            await _apiClient.DeleteAsync($"tasks/{id}");
        }

        public async Task<IEnumerable<TaskAttachmentItem>> GetAttachmentsAsync(int taskId)
        {
            var items = await _apiClient.GetAsync<List<ApiTaskAttachmentDto>>($"tasks/{taskId}/attachments");
            return (items ?? new List<ApiTaskAttachmentDto>()).Select(MapAttachment);
        }

        public async Task<TaskAttachmentItem> UploadAttachmentAsync(int taskId, string filePath, int? uploadedById)
        {
            await using var stream = File.OpenRead(filePath);
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(stream);
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(extension));
            content.Add(fileContent, "file", Path.GetFileName(filePath));

            var query = uploadedById.HasValue ? $"?uploadedById={uploadedById.Value}" : string.Empty;
            var response = await _apiClient.PostMultipartAsync<ApiTaskAttachmentDto>($"tasks/{taskId}/attachments{query}", content);
            return MapAttachment(response);
        }

        public async Task<byte[]> DownloadAttachmentAsync(int taskId, int attachmentId)
        {
            return await _apiClient.GetBytesAsync($"tasks/{taskId}/attachments/{attachmentId}");
        }

        public async Task DeleteAttachmentAsync(int taskId, int attachmentId)
        {
            await _apiClient.DeleteAsync($"tasks/{taskId}/attachments/{attachmentId}");
        }

        public static TaskItem MapToTaskItem(ApiTaskResponseDto task)
        {
            var assigneeIds = task.AssigneeIds ?? task.Assignees?.Select(a => a.UserId).ToList() ?? new List<int>();
            if (assigneeIds.Count == 0 && task.AssigneeId.HasValue)
            {
                assigneeIds.Add(task.AssigneeId.Value);
            }

            var assigneeName = task.AssigneeName;
            if (string.IsNullOrWhiteSpace(assigneeName) && task.Assignees?.Count > 0)
            {
                assigneeName = string.Join(", ", task.Assignees.Select(a => a.Name));
            }

            return new TaskItem
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                ProjectId = task.ProjectId,
                ProjectName = string.IsNullOrWhiteSpace(task.ProjectName) ? "Без проекта" : task.ProjectName,
                AuthorId = task.AuthorId,
                AuthorName = task.AuthorName,
                AssigneeId = task.AssigneeId,
                AssigneeName = assigneeName,
                AssigneeIds = assigneeIds,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                DueDate = task.DueDate,
                CompletedAt = task.CompletedAt,
                CommentsCount = task.CommentsCount,
                AttachmentsCount = task.AttachmentsCount
            };
        }

        public static TaskItem MapToTaskItem(TaskDto task)
        {
            var assigneeIds = task.AssigneeIds ?? task.Assignees?.Select(a => a.UserId).ToList() ?? new List<int>();
            if (assigneeIds.Count == 0 && task.AssigneeId.HasValue)
            {
                assigneeIds.Add(task.AssigneeId.Value);
            }

            var assigneeName = task.AssigneeName;
            if (string.IsNullOrWhiteSpace(assigneeName) && task.Assignees?.Count > 0)
            {
                assigneeName = string.Join(", ", task.Assignees.Select(a => a.Name));
            }

            return new TaskItem
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                ProjectId = task.ProjectId,
                ProjectName = string.IsNullOrWhiteSpace(task.ProjectName) ? "Без проекта" : task.ProjectName,
                AuthorId = task.AuthorId,
                AuthorName = task.AuthorName,
                AssigneeId = task.AssigneeId,
                AssigneeName = assigneeName,
                AssigneeIds = assigneeIds,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                DueDate = task.DueDate,
                CompletedAt = task.CompletedAt,
                CommentsCount = task.CommentsCount,
                AttachmentsCount = task.AttachmentsCount
            };
        }

        private static TaskDto MapToTaskDto(ApiTaskResponseDto response)
        {
            var assigneeIds = response.AssigneeIds ?? response.Assignees?.Select(a => a.UserId).ToList() ?? new List<int>();
            if (assigneeIds.Count == 0 && response.AssigneeId.HasValue)
            {
                assigneeIds.Add(response.AssigneeId.Value);
            }

            return new TaskDto
            {
                Id = response.Id,
                Title = response.Title,
                Description = response.Description,
                Status = response.Status,
                Priority = response.Priority,
                ProjectId = response.ProjectId,
                ProjectName = response.ProjectName,
                AuthorId = response.AuthorId,
                AuthorName = response.AuthorName,
                AssigneeId = response.AssigneeId,
                AssigneeName = response.AssigneeName,
                AssigneeIds = assigneeIds,
                Assignees = (response.Assignees ?? new List<ApiTaskAssigneeDto>())
                    .Select(a => new TaskAssigneeDto { UserId = a.UserId, Name = a.Name })
                    .ToList(),
                CreatedAt = response.CreatedAt,
                UpdatedAt = response.UpdatedAt,
                DueDate = response.DueDate,
                CompletedAt = response.CompletedAt,
                CommentsCount = response.CommentsCount,
                AttachmentsCount = response.AttachmentsCount,
                Attachments = (response.Attachments ?? new List<ApiTaskAttachmentDto>()).Select(MapAttachment).ToList()
            };
        }

        private static TaskAttachmentItem MapAttachment(ApiTaskAttachmentDto item)
        {
            return new TaskAttachmentItem
            {
                Id = item.Id,
                FileName = item.FileName,
                ContentType = item.ContentType,
                FileSize = item.FileSize,
                UploadedAt = item.UploadedAt,
                UploadedById = item.UploadedById,
                UploadedByName = item.UploadedByName
            };
        }

        private static string GetContentType(string extension) => extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}
