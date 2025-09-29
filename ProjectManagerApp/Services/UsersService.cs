using ProjectManagementSystem.WPF.Models;

namespace ProjectManagementSystem.WPF.Services
{
    public class UsersService : IUsersService
    {
        private readonly IApiClient _apiClient;

        public UsersService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IEnumerable<UserDto>> GetUsersAsync()
        {
            try
            {
                return await _apiClient.GetAsync<IEnumerable<UserDto>>("Users");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки пользователей: {ex.Message}", ex);
            }
        }

        public async Task<UserDto?> GetUserAsync(int id)
        {
            return await _apiClient.GetAsync<UserDto>($"Users/{id}");
        }

        public async Task<UserDto?> CreateUserAsync(UserDto user)
        {
            return await _apiClient.PostAsync<UserDto>("Users", user);
        }

        public async Task<UserDto?> UpdateUserAsync(UserDto user)
        {
            return await _apiClient.PutAsync<UserDto>($"Users/{user.Id}", user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await _apiClient.DeleteAsync($"Users/{id}");
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(int role)
        {
            var allUsers = await GetUsersAsync();
            return allUsers.Where(u => u.Role == role);
        }

        public async Task<IEnumerable<UserDto>> SearchUsersAsync(string searchTerm)
        {
            var allUsers = await GetUsersAsync();
            if (string.IsNullOrWhiteSpace(searchTerm))
                return allUsers;

            return allUsers.Where(u => 
                u.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        public static UserItem MapToUserItem(UserDto dto)
        {
            return new UserItem
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Role = dto.Role,
                CreatedAt = dto.CreatedAt,
                ManagedProjectsCount = dto.ManagedProjectsCount,
                AuthoredTasksCount = dto.AuthoredTasksCount,
                AssignedTasksCount = dto.AssignedTasksCount,
                CommentsCount = dto.CommentsCount
            };
        }
    }
}
