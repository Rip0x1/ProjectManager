using ProjectManagementSystem.WPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagementSystem.WPF.Services
{
    public interface IProjectsService
    {
        Task<IList<ProjectItem>> GetProjectsAsync();
    }

    public class ProjectsService : IProjectsService
    {
        private readonly IApiClient _apiClient;

        public ProjectsService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IList<ProjectItem>> GetProjectsAsync()
        {
            var raw = await _apiClient.GetAsync<List<ProjectDto>>("projects");
            if (raw == null)
            {
                return new List<ProjectItem>();
            }

            return raw.Select(p => new ProjectItem
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description ?? string.Empty,
                ManagerId = p.ManagerId,
                CreatedAt = p.CreatedAt,
                Deadline = p.Deadline,
                ManagerName = p.Manager != null ? ($"{p.Manager.FirstName} {p.Manager.LastName}") : "—",

                Status = GetRandomStatus(),
                ParticipantsCount = GetRandomCount(5, 25),
                TasksCount = GetRandomCount(10, 50),
                CommentsCount = GetRandomCount(20, 100)
            }).ToList();
        }

        private static int GetRandomStatus()
        {
            var random = new Random();
            return random.Next(0, 3);
        }

        private static int GetRandomCount(int min, int max)
        {
            var random = new Random();
            return random.Next(min, max + 1);
        }
    }
}


