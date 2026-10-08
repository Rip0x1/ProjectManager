using System.Net.Http;
using System.Threading.Tasks;

namespace ProjectManagementSystem.WPF.Services
{
    public interface IApiClient
    {
        Task<T> GetAsync<T>(string endpoint);
        Task<T> PostAsync<T>(string endpoint, object data);
        Task<T> PutAsync<T>(string endpoint, object data);
        Task<HttpResponseMessage> DeleteAsync(string endpoint);
        Task<T> PostMultipartAsync<T>(string endpoint, MultipartFormDataContent content);
        Task<byte[]> GetBytesAsync(string endpoint);
        void SetCurrentUser(int? userId);
    }
}
