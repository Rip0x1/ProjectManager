using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.API.Models.DTOs
{
    public class TaskCreateUpdateDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        [Required]
        public int Status { get; set; }
        
        [Required]
        public int Priority { get; set; }
        
        public int? ProjectId { get; set; }
        
        [Required]
        public int AuthorId { get; set; }
        
        public int? AssigneeId { get; set; }

        public List<int> AssigneeIds { get; set; } = new();
        
        public DateTime? DueDate { get; set; }
        
        public DateTime? CompletedAt { get; set; }
    }
}
