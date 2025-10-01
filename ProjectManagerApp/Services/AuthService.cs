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
                if (response != null && !string.IsNullOrEmpty(response.Email))
                {
                    _currentUser = response;
                    return response;
                }
                throw new Exception("Регистрация не удалась");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var message = ex.Message?.ToLowerInvariant();
                if (!string.IsNullOrEmpty(message) && message.Contains("already exists"))
                {
                    throw new Exception("Аккаунт с таким email уже существует");
                }
                throw new Exception("Некорректные данные регистрации");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ошибка регистрации: {ex.Message}");
            }
        }

        public async Task<AuthResponseDto> LoginAsync(string email, string password)
        {
            try
            {
                var loginDto = new LoginDto { Email = email, Password = password };
                var response = await _apiClient.PostAsync<AuthResponseDto>("auth/login", loginDto);

                if (response != null && !string.IsNullOrEmpty(response.Email))
                {
                    _currentUser = response;
                    return response;
                }
                else
                {
                    throw new Exception("Неверный email или пароль");
                }
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new Exception("Неверный email или пароль");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ошибка подключения к серверу: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка авторизации: {ex.Message}");
            }
        }

        public void Logout()
        {
            _currentUser = null;
        }

        public Task<bool> IsLoggedIn() => Task.FromResult(_currentUser != null);

        public int CurrentUserId => _currentUser?.UserId ?? 0;
        public int CurrentUserRole => _currentUser?.Role ?? 0;

        public string CurrentUserFirstName => _currentUser?.FirstName ?? string.Empty;
        public string CurrentUserEmail => _currentUser?.Email ?? string.Empty;
    }
}