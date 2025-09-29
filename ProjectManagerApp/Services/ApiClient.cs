using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ProjectManagementSystem.WPF.Services
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://localhost:7260/api/";

        public ApiClient()
        {
            var handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                await EnsureSuccess(response);
                var json = await response.Content.ReadAsStringAsync();
                return System.Text.Json.JsonSerializer.Deserialize<T>(json, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка API запроса к {endpoint}: {ex.Message}", ex);
            }
        }

        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, data);
            await EnsureSuccess(response);
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<T> PutAsync<T>(string endpoint, object data)
        {
            var response = await _httpClient.PutAsJsonAsync(endpoint, data);
            await EnsureSuccess(response);
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            var response = await _httpClient.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }

        private async Task EnsureSuccess(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var userFriendlyMessage = response.StatusCode switch
                {
                    HttpStatusCode.NotFound => "Данные не найдены",
                    HttpStatusCode.Unauthorized => "Недостаточно прав доступа",
                    HttpStatusCode.Forbidden => "Доступ запрещён",
                    HttpStatusCode.BadRequest => "Некорректный запрос",
                    HttpStatusCode.InternalServerError => "Ошибка сервера",
                    _ => $"Ошибка: {response.StatusCode}"
                };
                throw new HttpRequestException(userFriendlyMessage, null, response.StatusCode);
            }
        }
    }
}