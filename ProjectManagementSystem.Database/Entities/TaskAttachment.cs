using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Database.Entities
{
    public class TaskAttachment
    {
        public int Id { get; set; }
        public int TaskId { get; set; }

        [Required]
        [MaxLength(260)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(260)]
        public string StoredFileName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public int? UploadedById { get; set; }

        public virtual Task Task { get; set; } = null!;
        public virtual User? UploadedBy { get; set; }
    }
}
