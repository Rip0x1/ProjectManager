namespace ProjectManagementSystem.API.Models.DTOs
{
    public class CreateProjectCommentDto
    {
        public string Content { get; set; } = string.Empty;
        public int AuthorId { get; set; }
    }
}
