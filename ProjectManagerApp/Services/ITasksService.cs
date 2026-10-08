using ProjectManagementSystem.WPF.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectManagementSystem.WPF.Services
{
    public interface ITasksService
    {
        Task<IEnumerable<TaskItem>> GetTasksAsync();
        Task<TaskDto> GetTaskAsync(int id);
        Task<IEnumerable<TaskItem>> GetTasksByStatusAsync(int status);
        Task<IEnumerable<TaskItem>> GetTasksByPriorityAsync(int priority);
        Task<TaskDto> CreateTaskAsync(CreateUpdateTaskDto task);
        Task UpdateTaskAsync(int id, CreateUpdateTaskDto task);
        Task DeleteTaskAsync(int id);
        Task<IEnumerable<TaskAttachmentItem>> GetAttachmentsAsync(int taskId);
        Task<TaskAttachmentItem> UploadAttachmentAsync(int taskId, string filePath, int? uploadedById);
        Task<byte[]> DownloadAttachmentAsync(int taskId, int attachmentId);
        Task DeleteAttachmentAsync(int taskId, int attachmentId);
    }
}
