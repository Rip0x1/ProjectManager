using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ProjectManagementSystem.WPF.Services
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly object _schemeLock = new();
        private bool _alternateSchemeTried;

        public ApiClient(IConfiguration configuration)
        {
            var baseUrl = configuration["ApiBaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = "http://localhost:7260/api/";
            }
            if (!baseUrl.EndsWith("/", StringComparison.Ordinal))
            {
                baseUrl += "/";
            }

            var handler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromMinutes(5)
            };
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await SendWithFallback(() => _httpClient.GetAsync(endpoint));
                await EnsureSuccess(response, endpoint);
                var json = await response.Content.ReadAsStringAsync();
                return System.Text.Json.JsonSerializer.Deserialize<T>(json, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (HttpRequestException)
            {
                throw;
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
                var response = await SendWithFallback(() => _httpClient.PostAsJsonAsync(endpoint, data));
                await EnsureSuccess(response, endpoint);
                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (HttpRequestException)
            {
                throw;
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
                var response = await SendWithFallback(() => _httpClient.PutAsJsonAsync(endpoint, data));
                await EnsureSuccess(response, endpoint);
                
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return default(T);
                }
                
                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (HttpRequestException)
            {
                throw;
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
                return await SendWithFallback(() => _httpClient.DeleteAsync(endpoint));
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка DELETE запроса к {endpoint}: {ex.Message}");
                throw new Exception($"Ошибка DELETE запроса к {endpoint}: {ex.Message}", ex);
            }
        }

        public async Task<T> PostMultipartAsync<T>(string endpoint, MultipartFormDataContent content)
        {
            try
            {
                var response = await SendWithFallback(() => _httpClient.PostAsync(endpoint, content));
                await EnsureSuccess(response, endpoint);
                if (response.StatusCode == HttpStatusCode.NoContent)
                {
                    return default(T);
                }
                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки файла на {endpoint}: {ex.Message}", ex);
            }
        }

        public void SetCurrentUser(int? userId)
        {
            _httpClient.DefaultRequestHeaders.Remove("X-User-Id");
            if (userId.HasValue && userId.Value > 0)
            {
                _httpClient.DefaultRequestHeaders.Add("X-User-Id", userId.Value.ToString());
            }
        }

        public async Task<byte[]> GetBytesAsync(string endpoint)
        {
            try
            {
                var response = await SendWithFallback(() => _httpClient.GetAsync(endpoint));
                await EnsureSuccess(response, endpoint);
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка скачивания файла с {endpoint}: {ex.Message}", ex);
            }
        }

        private async Task<HttpResponseMessage> SendWithFallback(Func<Task<HttpResponseMessage>> send)
        {
            try
            {
                var response = await send();
                if (await IsHttpToHttpsMismatchAsync(response) && TrySwitchScheme())
                {
                    response.Dispose();
                    response = await send();
                }

                return response;
            }
            catch (Exception ex) when (IsTransportFailure(ex) && TrySwitchScheme())
            {
                return await send();
            }
        }

        private bool TrySwitchScheme()
        {
            lock (_schemeLock)
            {
                if (_alternateSchemeTried || _httpClient.BaseAddress == null)
                {
                    return false;
                }

                _alternateSchemeTried = true;
                var current = _httpClient.BaseAddress;
                var newScheme = current.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? "http" : "https";
                _httpClient.BaseAddress = new UriBuilder(current)
                {
                    Scheme = newScheme,
                    Port = current.Port
                }.Uri;
                return true;
            }
        }

        private static bool IsTransportFailure(Exception ex)
        {
            for (var current = ex; current != null; current = current.InnerException)
            {
                if (current is HttpRequestException or TaskCanceledException or IOException)
                {
                    return true;
                }
            }

            return false;
        }

        private static async Task<bool> IsHttpToHttpsMismatchAsync(HttpResponseMessage response)
        {
            if (response.StatusCode != HttpStatusCode.BadRequest)
            {
                return false;
            }

            await response.Content.LoadIntoBufferAsync();
            var body = await response.Content.ReadAsStringAsync();
            return body.Contains("HTTPS", StringComparison.OrdinalIgnoreCase)
                || body.Contains("HTTP request to an HTTPS", StringComparison.OrdinalIgnoreCase);
        }

        private async Task EnsureSuccess(HttpResponseMessage response, string endpoint)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var userFriendlyMessage = response.StatusCode switch
                {
                    HttpStatusCode.NotFound => "Данные не найдены",
                    HttpStatusCode.Unauthorized => IsAuthEndpoint(endpoint) ? "Неверный логин или пароль" : "Недостаточно прав доступа",
                    HttpStatusCode.Forbidden => "Доступ запрещён",
                    HttpStatusCode.BadRequest => GetBadRequestMessage(errorContent),
                    HttpStatusCode.InternalServerError => "Временная ошибка сервера. Попробуйте позже",
                    _ => $"Ошибка: {response.StatusCode}"
                };
                throw new HttpRequestException(userFriendlyMessage, null, response.StatusCode);
            }
        }

        private static bool IsAuthEndpoint(string endpoint)
        {
            return endpoint.Contains("auth/login", StringComparison.OrdinalIgnoreCase)
                || endpoint.Contains("auth/register", StringComparison.OrdinalIgnoreCase);
        }

        private string GetBadRequestMessage(string errorContent)
        {
            if (string.IsNullOrWhiteSpace(errorContent))
            {
                return "Проверьте правильность введенных данных";
            }

            var lowerContent = errorContent.ToLower();

            if ((lowerContent.Contains("логин") || lowerContent.Contains("login")) &&
                (lowerContent.Contains("уже существует") || lowerContent.Contains("уже используется")))
            {
                return "Пользователь с таким логином уже существует";
            }

            if ((lowerContent.Contains("логин") || lowerContent.Contains("login")) && lowerContent.Contains("некорректный"))
            {
                return "Введите корректный логин";
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