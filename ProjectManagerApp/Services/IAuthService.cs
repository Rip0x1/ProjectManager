using ProjectManagementSystem.WPF.Models;
using System.Threading.Tasks;

namespace ProjectManagementSystem.WPF.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(string email, string password);
        Task<bool> IsLoggedIn();
        void Logout();
        int? CurrentUserId { get; }
        string CurrentUserRole { get; }
    }
}