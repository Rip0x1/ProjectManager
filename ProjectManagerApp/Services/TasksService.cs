using ProjectManagementSystem.WPF.Models;

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
            try
            {
                var tasks = await _apiClient.GetAsync<IEnumerable<TaskDto>>("Tasks");
                return tasks.Select(MapToTaskItem);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки задач: {ex.Message}", ex);
            }
        }

        public async Task<TaskDto> GetTaskAsync(int id)
        {
            return await _apiClient.GetAsync<TaskDto>($"Tasks/{id}");
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByStatusAsync(int status)
        {
            var allTasks = await GetTasksAsync();
            return allTasks.Where(t => t.Status == status);
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByPriorityAsync(int priority)
        {
            var allTasks = await GetTasksAsync();
            return allTasks.Where(t => t.Priority == priority);
        }

        private static TaskItem MapToTaskItem(TaskDto task)
        {
            return new TaskItem
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                ProjectName = task.ProjectName,
                AuthorName = task.AuthorName,
                AssigneeName = task.AssigneeName,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                PlannedHours = task.PlannedHours,
                ActualHours = task.ActualHours,
                CommentsCount = task.CommentsCount
            };
        }
    }
}