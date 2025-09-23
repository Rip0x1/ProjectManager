using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Database.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public int Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Project> ManagedProjects { get; set; } = new List<Project>();
        public virtual ICollection<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();
        public virtual ICollection<Task> AuthoredTasks { get; set; } = new List<Task>();
        public virtual ICollection<Task> AssignedTasks { get; set; } = new List<Task>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
