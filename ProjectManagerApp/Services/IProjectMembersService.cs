using ProjectManagerApp.Models;

namespace ProjectManagerApp.Services
{
    public interface IProjectMembersService
    {
        Task<ProjectMembersResponse> GetProjectMembersAsync(int projectId, string? search = null, int page = 1, int pageSize = 10);
        Task<AvailableUsersResponse> GetAvailableUsersAsync(int projectId, string? search = null, int page = 1, int pageSize = 10);
        Task AddUserToProjectAsync(int projectId, int userId);
        Task RemoveUserFromProjectAsync(int projectId, int userId);
    }
}
