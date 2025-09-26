using ProjectManagementSystem.WPF.Models;
using System.Threading.Tasks;

namespace ProjectManagementSystem.WPF.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(string email, string password);
        Task<bool> IsLoggedIn();
        void Logout();
        int? CurrentUserId { get; }
        string CurrentUserRole { get; }
        string CurrentUserFirstName { get; }
        string CurrentUserEmail { get; }
    }
}