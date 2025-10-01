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
        Task<ProjectDto> GetProjectAsync(int id);
        Task<ProjectDto> CreateProjectAsync(CreateUpdateProjectDto project);
        Task UpdateProjectAsync(int id, CreateUpdateProjectDto project);
        Task DeleteProjectAsync(int id);
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

                Status = p.Status,
                ParticipantsCount = 0, 
                TasksCount = 0, 
                CommentsCount = 0 
            }).ToList();
        }

        public async Task<ProjectDto> GetProjectAsync(int id)
        {
            return await _apiClient.GetAsync<ProjectDto>($"projects/{id}");
        }

        public async Task<ProjectDto> CreateProjectAsync(CreateUpdateProjectDto project)
        {
            return await _apiClient.PostAsync<ProjectDto>("projects", project);
        }

        public async Task UpdateProjectAsync(int id, CreateUpdateProjectDto project)
        {
            await _apiClient.PutAsync<ProjectDto>($"projects/{id}", project);
        }

        public async Task DeleteProjectAsync(int id)
        {
            await _apiClient.DeleteAsync($"projects/{id}");
        }
    }
}


