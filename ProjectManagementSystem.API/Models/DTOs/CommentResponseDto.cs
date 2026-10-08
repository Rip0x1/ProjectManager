using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.API.Models.DTOs
{
    public class CommentResponseDto
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public int? TaskId { get; set; }
        public int AuthorId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public CommentUserResponceDto CommentUserResponceDto { get; set; }
        public CommentTaskReponseDto CommentTaskReponseDto { get; set; }
        public List<CommentAttachmentDto> Attachments { get; set; } = new();
    }

    public class CommentAttachmentDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public int? UploadedById { get; set; }
    }

    public class CommentUserResponceDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Login { get; set; }
    }

    public class CommentTaskReponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; }
    }

}
