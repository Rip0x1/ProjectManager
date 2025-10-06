using ProjectManagementSystem.WPF.Models;
using ProjectManagerApp.Models;
using System.Net.Http;

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

        public async Task<UserDto> CreateUserAsync(CreateUserDto createDto)
        {
            try
            {
                var apiDto = new ApiUserCreateDto
                {
                    FirstName = createDto.FirstName?.Trim() ?? string.Empty,
                    LastName = createDto.LastName?.Trim() ?? string.Empty,
                    Email = createDto.Email?.Trim() ?? string.Empty,
                    Password = createDto.Password ?? string.Empty,
                    Role = createDto.Role
                };
                
                return await _apiClient.PostAsync<UserDto>("Users", apiDto);
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка создания пользователя: {ex.Message}", ex);
            }
        }

        public async Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateDto)
        {
            try
            {
                var apiDto = new ApiUserUpdateDto
                {
                    FirstName = updateDto.FirstName?.Trim() ?? string.Empty,
                    LastName = updateDto.LastName?.Trim() ?? string.Empty,
                    Email = updateDto.Email?.Trim() ?? string.Empty,
                    Password = string.IsNullOrWhiteSpace(updateDto.Password) ? null : updateDto.Password,
                    Role = updateDto.Role
                };
                
                return await _apiClient.PutAsync<UserDto>($"Users/{id}", apiDto);
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка обновления пользователя: {ex.Message}", ex);
            }
        }

        public async Task DeleteUserAsync(int id)
        {
            try
            {
                var response = await _apiClient.DeleteAsync($"Users/{id}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    
                    if (errorContent.Contains("управляет проектами"))
                    {
                        throw new Exception("Нельзя удалить пользователя, который управляет проектами. Сначала переназначьте проекты другому менеджеру.");
                    }
                    
                    throw new Exception("Ошибка удаления пользователя. Попробуйте позже.");
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("управляет проектами"))
                {
                    throw;
                }
                throw new Exception($"Ошибка удаления пользователя: {ex.Message}", ex);
            }
        }
    }
}
