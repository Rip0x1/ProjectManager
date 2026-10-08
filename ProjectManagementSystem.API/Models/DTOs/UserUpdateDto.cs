using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.API.Models.DTOs
{
    public class UserUpdateDto
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9._-]{3,100}$")]
        [StringLength(100, MinimumLength = 3)]
        public string Login { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 6)]
        public string? Password { get; set; }

        [Range(0, 2)]
        public int Role { get; set; } = 0;
    }
}
