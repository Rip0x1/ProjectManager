using ProjectManagementSystem.WPF.Models;

namespace ProjectManagementSystem.WPF.Services
{
    public interface IUsersService
    {
        Task<IEnumerable<UserDto>> GetUsersAsync();
        Task<UserDto?> GetUserAsync(int id);
        Task<UserDto> CreateUserAsync(CreateUserDto createDto);
        Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateDto);
        Task DeleteUserAsync(int id);
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(int role);
        Task<IEnumerable<UserDto>> SearchUsersAsync(string searchTerm);
    }
}
