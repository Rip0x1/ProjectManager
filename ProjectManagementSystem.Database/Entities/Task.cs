using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Database.Entities
{
    public class Task
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        public int Status { get; set; }
        public int Priority { get; set; }

        public int? ProjectId { get; set; }
        public int AuthorId { get; set; }
        public int? AssigneeId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }

        public virtual Project? Project { get; set; }
        public virtual User Author { get; set; }
        public virtual User? Assignee { get; set; }
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<TaskAssignee> Assignees { get; set; } = new List<TaskAssignee>();
        public virtual ICollection<TaskAttachment> Attachments { get; set; } = new List<TaskAttachment>();
    }
}
