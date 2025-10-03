using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.API.Models.DTOs
{
    public class CreateCommentDto
    {
        [Required]
        [StringLength(2000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;
        
        public int AuthorId { get; set; }
    }
}
