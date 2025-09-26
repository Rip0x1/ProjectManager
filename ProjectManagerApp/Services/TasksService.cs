using ProjectManagementSystem.WPF.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementSystem.WPF.Services
{
    public class TasksService : ITasksService
    {
        private readonly IApiClient _apiClient;

        public TasksService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IEnumerable<TaskItem>> GetTasksAsync()
        {
            var tasks = await _apiClient.GetAsync<IEnumerable<TaskDto>>("api/tasks");
            return tasks.Select(MapToTaskItem);
        }

        public async Task<TaskDto> GetTaskAsync(int id)
        {
            return await _apiClient.GetAsync<TaskDto>($"api/tasks/{id}");
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByStatusAsync(int status)
        {
            var tasks = await _apiClient.GetAsync<IEnumerable<TaskDto>>($"api/tasks/status/{status}");
            return tasks.Select(MapToTaskItem);
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByPriorityAsync(int priority)
        {
            var tasks = await _apiClient.GetAsync<IEnumerable<TaskDto>>($"api/tasks/priority/{priority}");
            return tasks.Select(MapToTaskItem);
        }

        private TaskItem MapToTaskItem(TaskDto task)
        {
            return new TaskItem
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                StatusText = GetStatusText(task.Status),
                PriorityText = GetPriorityText(task.Priority),
                ProjectName = task.Project?.Name ?? "Неизвестный проект",
                AuthorName = task.Author != null ? $"{task.Author.FirstName} {task.Author.LastName}" : "Неизвестный автор",
                AssigneeName = task.Assignee != null ? $"{task.Assignee.FirstName} {task.Assignee.LastName}" : "Не назначен",
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                PlannedHours = task.PlannedHours,
                ActualHours = task.ActualHours,
                StatusColor = GetStatusColor(task.Status),
                PriorityColor = GetPriorityColor(task.Priority)
            };
        }

        private string GetStatusText(int status)
        {
            return status switch
            {
                0 => "Новая",
                1 => "В работе",
                2 => "На проверке",
                3 => "Завершена",
                4 => "Отменена",
                _ => "Неизвестно"
            };
        }

        private string GetPriorityText(int priority)
        {
            return priority switch
            {
                1 => "Низкий",
                2 => "Средний",
                3 => "Высокий",
                4 => "Критический",
                _ => "Неизвестно"
            };
        }

        private string GetStatusColor(int status)
        {
            return status switch
            {
                0 => "#2196F3", 
                1 => "#FF9800", 
                2 => "#9C27B0", 
                3 => "#4CAF50", 
                4 => "#F44336", 
                _ => "#9E9E9E"  
            };
        }

        private string GetPriorityColor(int priority)
        {
            return priority switch
            {
                1 => "#4CAF50", 
                2 => "#FF9800",
                3 => "#F44336",
                4 => "#9C27B0", 
                _ => "#9E9E9E" 
            };
        }
    }
}
