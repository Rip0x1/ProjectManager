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
        
        [Required]
        public int ProjectId { get; set; }
        
        [Required]
        public int AuthorId { get; set; }
        
        public int? AssigneeId { get; set; }
        
        public decimal? PlannedHours { get; set; }
        
        public decimal? ActualHours { get; set; }
    }
}

