using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Database.Entities
{
    public class ProjectUser
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int UserId { get; set; }

        [MaxLength(100)]
        public string RoleInProject { get; set; }

        public virtual Project Project { get; set; }
        public virtual User User { get; set; }
    }
}