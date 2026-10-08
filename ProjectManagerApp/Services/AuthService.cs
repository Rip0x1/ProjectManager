using ProjectManagementSystem.WPF.Models;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProjectManagementSystem.WPF.Services
{
    public class AuthService : IAuthService
    {
        private readonly IApiClient _apiClient;
        private AuthResponseDto _currentUser;

        public AuthService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            try
            {
                var response = await _apiClient.PostAsync<AuthResponseDto>("auth/register", dto);
                if (response != null && !string.IsNullOrEmpty(response.Login))
                {
                    _currentUser = response;
                    _apiClient.SetCurrentUser(response.UserId);
                    return response;
                }
                throw new Exception("Регистрация не удалась");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var message = ex.Message?.ToLowerInvariant();
                if (!string.IsNullOrEmpty(message) && message.Contains("already exists"))
                {
                    throw new Exception("Пользователь с таким логином уже существует");
                }
                if (!string.IsNullOrEmpty(message) && (message.Contains("login") || message.Contains("логин")))
                {
                    throw new Exception("Введите корректный логин");
                }
                throw new Exception("Проверьте правильность введенных данных");
            }
            catch (HttpRequestException)
            {
                throw new Exception("Ошибка подключения к серверу. Проверьте, что API запущен");
            }
            catch (Exception ex) when (ex.Message.Contains("API") || ex.Message.Contains("сервер"))
            {
                throw new Exception("Ошибка подключения к серверу. Проверьте, что API запущен");
            }
        }

        public async Task<AuthResponseDto> LoginAsync(string login, string password)
        {
            try
            {
                var loginDto = new LoginDto { Login = login, Password = password };
                var response = await _apiClient.PostAsync<AuthResponseDto>("auth/login", loginDto);

                if (response != null && !string.IsNullOrEmpty(response.Login))
                {
                    _currentUser = response;
                    _apiClient.SetCurrentUser(response.UserId);
                    return response;
                }
                else
                {
                    throw new Exception("Неверный логин или пароль");
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                    throw new Exception("Неверный логин или пароль");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                    throw new Exception("Неверный логин или пароль");
            }
            catch (HttpRequestException)
            {
                throw new Exception("Ошибка подключения к серверу. Проверьте, что API запущен");
            }
            catch (Exception ex) when (ex.Message.Contains("API") || ex.Message.Contains("сервер"))
            {
                throw new Exception("Ошибка подключения к серверу. Проверьте, что API запущен");
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _apiClient.PostAsync<object>("auth/logout", new { });
            }
            catch
            {
            }
            finally
            {
                _currentUser = null;
                _apiClient.SetCurrentUser(null);
            }
        }

        public Task<bool> IsLoggedIn() => Task.FromResult(_currentUser != null);

        public int CurrentUserId => _currentUser?.UserId ?? 0;
        public int CurrentUserRole => _currentUser?.Role ?? 0;

        public string CurrentUserFirstName => _currentUser?.FirstName ?? string.Empty;
        public string CurrentUserLogin => _currentUser?.Login ?? string.Empty;
        public UserDto? CurrentUser => _currentUser != null ? new UserDto
        {
            Id = _currentUser.UserId,
            FirstName = _currentUser.FirstName,
            LastName = _currentUser.LastName,
            Login = _currentUser.Login,
            Role = _currentUser.Role,
            CreatedAt = DateTime.UtcNow 
        } : null;
    }
}