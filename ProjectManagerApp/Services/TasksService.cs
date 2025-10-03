using ProjectManagementSystem.WPF.Models;
using ProjectManagerApp.Models;

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
                var response = await _apiClient.GetAsync<ApiTasksResponse>("tasks?page=1&pageSize=1000");
                return response.Tasks.Select(MapToTaskItem);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки задач: {ex.Message}", ex);
            }
        }

        public async Task<TaskDto> GetTaskAsync(int id)
        {
            var response = await _apiClient.GetAsync<ApiTaskResponseDto>($"tasks/{id}");
            return new TaskDto
            {
                Id = response.Id,
                Title = response.Title,
                Description = response.Description,
                Status = response.Status,
                Priority = response.Priority,
                ProjectId = response.ProjectId,
                ProjectName = response.ProjectName,
                AuthorId = response.AuthorId,
                AuthorName = response.AuthorName,
                AssigneeId = response.AssigneeId,
                AssigneeName = response.AssigneeName,
                CreatedAt = response.CreatedAt,
                UpdatedAt = response.UpdatedAt,
                PlannedHours = response.PlannedHours,
                ActualHours = response.ActualHours,
                CommentsCount = response.CommentsCount
            };
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

        public async Task<TaskDto> CreateTaskAsync(CreateUpdateTaskDto task)
        {
            return await _apiClient.PostAsync<TaskDto>("tasks", task);
        }

        public async Task UpdateTaskAsync(int id, CreateUpdateTaskDto task)
        {
            await _apiClient.PutAsync<TaskDto>($"tasks/{id}", task);
        }

        public async Task DeleteTaskAsync(int id)
        {
            await _apiClient.DeleteAsync($"tasks/{id}");
        }

        private static TaskItem MapToTaskItem(ApiTaskResponseDto task)
        {
            return new TaskItem
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                ProjectId = task.ProjectId,
                ProjectName = task.ProjectName,
                AuthorId = task.AuthorId,
                AuthorName = task.AuthorName,
                AssigneeId = task.AssigneeId,
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