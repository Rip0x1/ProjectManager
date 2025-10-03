using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.API.Models.DTOs
{
    public class UserCreateDto
    {
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Range(0, 2)]
        public int Role { get; set; } = 0;
    }
}
