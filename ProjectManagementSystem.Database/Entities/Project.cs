using ProjectManagementSystem.Database.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Database.Entities
{
    public class Project
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        public int ManagerId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? Deadline { get; set; }

        public virtual User Manager { get; set; }
        public virtual ICollection<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();
        public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
    }
}