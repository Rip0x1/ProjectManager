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
            catch (HttpRequestException ex) when (ex.Message.Contains("401"))
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

        public int? CurrentUserId => _currentUser?.UserId;
        public string CurrentUserRole => _currentUser?.Role switch
        {
            0 => "Пользователь",
            1 => "Менеджер",
            2 => "Администратор",
            _ => "Неизвестно"
        };
    }
}