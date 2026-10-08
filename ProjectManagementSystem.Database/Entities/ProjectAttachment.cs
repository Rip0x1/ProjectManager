using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Database.Entities
{
    public class ProjectAttachment
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }

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

        public virtual Project Project { get; set; } = null!;
        public virtual User? UploadedBy { get; set; }
    }
}
