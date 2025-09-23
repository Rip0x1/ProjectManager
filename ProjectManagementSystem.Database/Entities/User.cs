using ProjectManagementSystem.Database.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public int Role {  get; set; } // 0 - User, 1 - Manager, 2 - Admin

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<ProjectUser> ProjectUsers { get; set; }
        public virtual ICollection<Task> AuthoredTasks { get; set; }
        public virtual ICollection<Task> AssignedTasks { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }

    }
}
