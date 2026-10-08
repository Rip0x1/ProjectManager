using ProjectManagerApp.Models;
using ProjectManagementSystem.WPF.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagerApp.Services
{
    public class UserProjectsService : IUserProjectsService
    {
        private readonly IApiClient _apiClient;

        public UserProjectsService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IList<UserProjectItem>> GetUserProjectsAsync(int userId)
        {
            var raw = await _apiClient.GetAsync<List<UserProjectDto>>($"projects/user/{userId}");
            if (raw == null)
            {
                return new List<UserProjectItem>();
            }

            return raw.Select(p => new UserProjectItem
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                ManagerId = p.ManagerId,
                CreatedAt = p.CreatedAt,
                Deadline = p.Deadline,
                Status = p.Status,
                ManagerName = p.Manager != null ? ($"{p.Manager.FirstName} {p.Manager.LastName}") : "—",
                RoleInProject = p.RoleInProject,
                JoinedAt = p.JoinedAt,
                ParticipantsCount = p.ParticipantsCount,
                TasksCount = p.TasksCount,
                CommentsCount = p.CommentsCount,
                Progress = p.Progress
            }).ToList();
        }

        public async Task<UserProjectDto> GetUserProjectAsync(int projectId)
        {
            return await _apiClient.GetAsync<UserProjectDto>($"projects/{projectId}");
        }
    }
}
