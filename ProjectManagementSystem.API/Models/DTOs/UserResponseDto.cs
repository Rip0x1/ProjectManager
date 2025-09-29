namespace ProjectManagementSystem.API.Models.DTOs
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ManagedProjectsCount { get; set; }
        public int AuthoredTasksCount { get; set; }
        public int AssignedTasksCount { get; set; }
        public int CommentsCount { get; set; }
    }
}
