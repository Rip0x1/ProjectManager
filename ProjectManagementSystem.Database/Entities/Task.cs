using ProjectManagementSystem.Database.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManagementSystem.Database.Entities
{
    public class Task
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        public string Description { get; set; }
        public int Status { get; set; } // 0 - ToDo, 1 - InProgress, 2 - CodeReview, 3 - Done
        public int Priority { get; set; } // 0 - Low, 1 - Medium, 2 - High
        public int ProjectId { get; set; }
        public int AuthorId { get; set; }
        public int? AssigneeId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set;} = DateTime.UtcNow;
        public decimal PlannedHours { get; set; }
        public decimal? ActualHours { get; set; }

        public virtual Project Project { get; set; }
        public virtual User Author { get; set; }
        public virtual User Assignee { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }

    }
}
