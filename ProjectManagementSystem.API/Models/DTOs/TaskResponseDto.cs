namespace ProjectManagementSystem.API.Models.DTOs
{
    public class TaskResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Status { get; set; }
        public int Priority { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public int? AssigneeId { get; set; }
        public string? AssigneeName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public decimal PlannedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public int CommentsCount { get; set; }
    }
}
