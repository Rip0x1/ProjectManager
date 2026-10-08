using System.Collections.Generic;

namespace ProjectManagerApp.Models
{
    public class ApiTaskAssigneeDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class ApiTaskAttachmentDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public int? UploadedById { get; set; }
        public string? UploadedByName { get; set; }
    }

    public class ApiTaskResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Status { get; set; }
        public int Priority { get; set; }
        public int? ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public int? AssigneeId { get; set; }
        public string? AssigneeName { get; set; }
        public List<int> AssigneeIds { get; set; } = new();
        public List<ApiTaskAssigneeDto> Assignees { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int CommentsCount { get; set; }
        public int AttachmentsCount { get; set; }
        public List<ApiTaskAttachmentDto> Attachments { get; set; } = new();
    }

    public class ApiTasksResponse
    {
        public List<ApiTaskResponseDto> Tasks { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }
}
