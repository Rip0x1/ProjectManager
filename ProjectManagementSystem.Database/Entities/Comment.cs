using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Database.Entities
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        public int? TaskId { get; set; }
        public int AuthorId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Task Task { get; set; }
        public virtual User Author { get; set; }
    }
}