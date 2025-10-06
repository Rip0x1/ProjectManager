using System;
using System.Diagnostics;
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
                Timeout = TimeSpan.FromMinutes(5) 
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
                Debug.WriteLine($"Ошибка API запроса к {endpoint}: {ex.Message}", ex);
                throw new Exception($"Ошибка API запроса к {endpoint}: {ex.Message}", ex);
            }
        }

        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(endpoint, data);
                await EnsureSuccess(response);
                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка POST запроса к {endpoint}: {ex.Message}", ex);
            }
        }

        public async Task<T> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(endpoint, data);
                await EnsureSuccess(response);
                
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return default(T);
                }
                
                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка PUT запроса к {endpoint}: {ex.Message}", ex);
                throw new Exception($"Ошибка PUT запроса к {endpoint}: {ex.Message}", ex);
            }
        }

        public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);
                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка DELETE запроса к {endpoint}: {ex.Message}");
                throw new Exception($"Ошибка DELETE запроса к {endpoint}: {ex.Message}", ex);
            }
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
                    HttpStatusCode.BadRequest => GetBadRequestMessage(errorContent),
                    HttpStatusCode.InternalServerError => "Временная ошибка сервера. Попробуйте позже",
                    _ => $"Ошибка: {response.StatusCode}"
                };
                throw new HttpRequestException(userFriendlyMessage, null, response.StatusCode);
            }
        }

        private string GetBadRequestMessage(string errorContent)
        {
            if (string.IsNullOrWhiteSpace(errorContent))
            {
                return "Проверьте правильность введенных данных";
            }

            var lowerContent = errorContent.ToLower();

            if (lowerContent.Contains("email") && lowerContent.Contains("уже существует"))
            {
                return "Пользователь с таким email уже существует";
            }

            if (lowerContent.Contains("email") && lowerContent.Contains("некорректный"))
            {
                return "Введите корректный email адрес";
            }

            if (lowerContent.Contains("имя") && lowerContent.Contains("обязательно"))
            {
                return "Имя является обязательным полем";
            }

            if (lowerContent.Contains("фамилия") && lowerContent.Contains("обязательно"))
            {
                return "Фамилия является обязательным полем";
            }

            if (lowerContent.Contains("пароль") && lowerContent.Contains("короткий"))
            {
                return "Пароль должен содержать минимум 6 символов";
            }

            if (lowerContent.Contains("роль") && lowerContent.Contains("неверная"))
            {
                return "Выберите корректную роль пользователя";
            }

            if (lowerContent.Contains("название") && lowerContent.Contains("уже существует"))
            {
                return "Проект с таким названием уже существует";
            }

            if (lowerContent.Contains("название") && lowerContent.Contains("обязательно"))
            {
                return "Название проекта является обязательным полем";
            }

            if (lowerContent.Contains("менеджер") && lowerContent.Contains("не найден"))
            {
                return "Выбранный менеджер не найден";
            }

            if (lowerContent.Contains("менеджер") && lowerContent.Contains("обязательно"))
            {
                return "Менеджер проекта является обязательным полем";
            }

            if (lowerContent.Contains("заголовок") && lowerContent.Contains("обязательно"))
            {
                return "Заголовок задачи является обязательным полем";
            }

            if (lowerContent.Contains("проект") && lowerContent.Contains("не найден"))
            {
                return "Выбранный проект не найден";
            }

            if (lowerContent.Contains("проект") && lowerContent.Contains("обязательно"))
            {
                return "Проект является обязательным полем";
            }

            if (lowerContent.Contains("приоритет") && lowerContent.Contains("неверный"))
            {
                return "Выберите корректный приоритет задачи";
            }

            if (lowerContent.Contains("статус") && lowerContent.Contains("неверный"))
            {
                return "Выберите корректный статус задачи";
            }

            if (lowerContent.Contains("содержимое") && lowerContent.Contains("обязательно"))
            {
                return "Содержимое комментария является обязательным полем";
            }

            if (lowerContent.Contains("задача") && lowerContent.Contains("не найдена"))
            {
                return "Задача не найдена";
            }

            if (lowerContent.Contains("modelstate") || lowerContent.Contains("validation"))
            {
                return "Проверьте правильность введенных данных";
            }

            return "Проверьте правильность введенных данных";
        }
    }
}