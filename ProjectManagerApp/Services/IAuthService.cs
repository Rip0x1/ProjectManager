using ProjectManagementSystem.WPF.Models;
using System.Threading.Tasks;

namespace ProjectManagementSystem.WPF.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(string login, string password);
        Task<bool> IsLoggedIn();
        Task LogoutAsync();
        int CurrentUserId { get; }
        int CurrentUserRole { get; }
        string CurrentUserFirstName { get; }
        string CurrentUserLogin { get; }
        UserDto? CurrentUser { get; }
    }
}