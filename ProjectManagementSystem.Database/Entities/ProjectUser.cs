using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManagementSystem.Database.Entities
{
    public class ProjectUser
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int UserId { get; set; }

        [MaxLength(100)]
        public string RoleInProject { get; set; } // "Developer", "Tester", "Designer"

        public virtual Project Project { get; set; }
        public virtual User User { get; set; }

    }
}
