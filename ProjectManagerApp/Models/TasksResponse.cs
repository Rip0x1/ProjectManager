using System.Collections.Generic;
using ProjectManagementSystem.WPF.Models;

namespace ProjectManagerApp.Models
{
    public class TasksResponse
    {
        public List<TaskItem> Tasks { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }
}
