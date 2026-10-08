using ProjectManagerApp.Models;
using ProjectManagementSystem.WPF.Services;

namespace ProjectManagerApp.Services
{
    public class ProjectMembersService : IProjectMembersService
    {
        private readonly IApiClient _apiClient;

        public ProjectMembersService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ProjectMembersResponse> GetProjectMembersAsync(int projectId, string? search = null, int page = 1, int pageSize = 10)
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(search))
                queryParams.Add($"search={Uri.EscapeDataString(search)}");
            queryParams.Add($"page={page}");
            queryParams.Add($"pageSize={pageSize}");

            var url = $"ProjectMembers/project/{projectId}";
            if (queryParams.Any())
                url += "?" + string.Join("&", queryParams);

            return await _apiClient.GetAsync<ProjectMembersResponse>(url);
        }

        public async Task<AvailableUsersResponse> GetAvailableUsersAsync(int projectId, string? search = null, int page = 1, int pageSize = 10)
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(search))
                queryParams.Add($"search={Uri.EscapeDataString(search)}");
            queryParams.Add($"page={page}");
            queryParams.Add($"pageSize={pageSize}");

            var url = $"ProjectMembers/available-users/{projectId}";
            if (queryParams.Any())
                url += "?" + string.Join("&", queryParams);

            return await _apiClient.GetAsync<AvailableUsersResponse>(url);
        }

        public async Task AddUserToProjectAsync(int projectId, int userId)
        {
            await _apiClient.PostAsync<object>($"ProjectMembers/project/{projectId}/user/{userId}", new { });
        }

        public async Task RemoveUserFromProjectAsync(int projectId, int userId)
        {
            await _apiClient.DeleteAsync($"ProjectMembers/project/{projectId}/user/{userId}");
        }
    }
}
