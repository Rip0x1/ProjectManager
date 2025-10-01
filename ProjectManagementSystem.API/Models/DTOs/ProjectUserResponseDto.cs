using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.API.Models.DTOs
{
    public class ProjectUserResponseDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public string RoleInProject { get; set; }
        public ProjectsAuthorResponceDto ProjectsAuthorResponceDto { get; set; }
        public ProjectsResponceDto ProjectsResponceDto { get; set; }
    }

    public class ProjectsAuthorResponceDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }

    public class ProjectsResponceDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

}
