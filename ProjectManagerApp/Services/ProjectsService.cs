using ProjectManagementSystem.WPF.Models;
using ProjectManagerApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ProjectManagementSystem.WPF.Services
{
    public interface IProjectsService
    {
        Task<IList<ProjectItem>> GetProjectsAsync();
        Task<ProjectDto> GetProjectAsync(int id);
        Task<ProjectDto> CreateProjectAsync(CreateUpdateProjectDto project);
        Task UpdateProjectAsync(int id, CreateUpdateProjectDto project);
        Task DeleteProjectAsync(int id);
        Task<IEnumerable<TaskAttachmentItem>> GetAttachmentsAsync(int projectId);
        Task<TaskAttachmentItem> UploadAttachmentAsync(int projectId, string filePath, int? uploadedById);
        Task<byte[]> DownloadAttachmentAsync(int projectId, int attachmentId);
        Task DeleteAttachmentAsync(int projectId, int attachmentId);
    }

    public class ProjectsService : IProjectsService
    {
        private readonly IApiClient _apiClient;

        public ProjectsService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IList<ProjectItem>> GetProjectsAsync()
        {
            var raw = await _apiClient.GetAsync<List<ProjectDto>>("projects");
            if (raw == null)
            {
                return new List<ProjectItem>();
            }

            return raw.Select(p => new ProjectItem
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description ?? string.Empty,
                ManagerId = p.ManagerId,
                CreatedAt = p.CreatedAt,
                Deadline = p.Deadline,
                ManagerName = p.Manager != null ? ($"{p.Manager.FirstName} {p.Manager.LastName}") : "—",
                Status = p.Status,
                ParticipantsCount = p.ParticipantsCount,
                TasksCount = p.TasksCount,
                CommentsCount = p.CommentsCount 
            }).ToList();
        }

        public async Task<ProjectDto> GetProjectAsync(int id)
        {
            return await _apiClient.GetAsync<ProjectDto>($"projects/{id}");
        }

        public async Task<ProjectDto> CreateProjectAsync(CreateUpdateProjectDto project)
        {
            return await _apiClient.PostAsync<ProjectDto>("projects", project);
        }

        public async Task UpdateProjectAsync(int id, CreateUpdateProjectDto project)
        {
            await _apiClient.PutAsync<ProjectDto>($"projects/{id}", project);
        }

        public async Task DeleteProjectAsync(int id)
        {
            await _apiClient.DeleteAsync($"projects/{id}");
        }

        public async Task<IEnumerable<TaskAttachmentItem>> GetAttachmentsAsync(int projectId)
        {
            var items = await _apiClient.GetAsync<List<ApiTaskAttachmentDto>>($"projects/{projectId}/attachments");
            return (items ?? new List<ApiTaskAttachmentDto>()).Select(MapAttachment);
        }

        public async Task<TaskAttachmentItem> UploadAttachmentAsync(int projectId, string filePath, int? uploadedById)
        {
            await using var stream = File.OpenRead(filePath);
            using var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(stream);
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(extension));
            content.Add(fileContent, "file", Path.GetFileName(filePath));

            var query = uploadedById.HasValue ? $"?uploadedById={uploadedById.Value}" : string.Empty;
            var response = await _apiClient.PostMultipartAsync<ApiTaskAttachmentDto>($"projects/{projectId}/attachments{query}", content);
            return MapAttachment(response);
        }

        public async Task<byte[]> DownloadAttachmentAsync(int projectId, int attachmentId)
        {
            return await _apiClient.GetBytesAsync($"projects/{projectId}/attachments/{attachmentId}");
        }

        public async Task DeleteAttachmentAsync(int projectId, int attachmentId)
        {
            await _apiClient.DeleteAsync($"projects/{projectId}/attachments/{attachmentId}");
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


