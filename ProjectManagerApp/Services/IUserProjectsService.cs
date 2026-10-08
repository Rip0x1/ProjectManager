using ProjectManagerApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectManagerApp.Services
{
    public interface IUserProjectsService
    {
        Task<IList<UserProjectItem>> GetUserProjectsAsync(int userId);
        Task<UserProjectDto> GetUserProjectAsync(int projectId);
    }
}
