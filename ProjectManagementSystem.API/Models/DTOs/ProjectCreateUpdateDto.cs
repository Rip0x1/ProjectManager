using System;

namespace ProjectManagementSystem.API.Models.DTOs
{
    public class ProjectCreateUpdateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int ManagerId { get; set; }
        public int Status { get; set; } = 0;
        public DateTime? Deadline { get; set; }
    }
}

