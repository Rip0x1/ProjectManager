using System;

namespace ProjectManagementSystem.WPF.Models
{
    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public int Priority { get; set; }
        public int ProjectId { get; set; }
        public int AuthorId { get; set; }
        public int? AssigneeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal PlannedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public ProjectDto Project { get; set; }
        public UserDto Author { get; set; }
        public UserDto Assignee { get; set; }
    }

    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string StatusText { get; set; }
        public string PriorityText { get; set; }
        public string ProjectName { get; set; }
        public string AuthorName { get; set; }
        public string AssigneeName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal PlannedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public string StatusColor { get; set; }
        public string PriorityColor { get; set; }
    }
}
