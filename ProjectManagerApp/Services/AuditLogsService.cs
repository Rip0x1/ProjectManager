using System.Net.Http.Json;
using System.Text;
using ProjectManagementSystem.WPF.Models;

namespace ProjectManagementSystem.WPF.Services
{
    public class AuditLogsService : IAuditLogsService
    {
        private readonly IApiClient _apiClient;

        public AuditLogsService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<PagedAuditLogsDto> GetLogsAsync(
            string? searchTerm,
            string? action,
            string? entityType,
            DateTime? fromDate,
            DateTime? toDate,
            int pageNumber,
            int pageSize)
        {
            var query = BuildQuery(searchTerm, action, entityType, fromDate, toDate);
            query.Append($"&pageNumber={pageNumber}");
            query.Append($"&pageSize={pageSize}");

            var result = await _apiClient.GetAsync<PagedAuditLogsDto>(query.ToString());
            return result ?? new PagedAuditLogsDto();
        }

        public async Task<int> ClearLogsAsync(
            string? searchTerm,
            string? action,
            string? entityType,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = BuildQuery(searchTerm, action, entityType, fromDate, toDate);
            var response = await _apiClient.DeleteAsync(query.ToString());
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Не удалось очистить журнал");
            }

            var result = await response.Content.ReadFromJsonAsync<ClearAuditLogsResultDto>();
            return result?.DeletedCount ?? 0;
        }

        private static StringBuilder BuildQuery(
            string? searchTerm,
            string? action,
            string? entityType,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = new StringBuilder("auditlogs?");

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query.Append($"searchTerm={Uri.EscapeDataString(searchTerm.Trim())}&");
            }

            if (!string.IsNullOrWhiteSpace(action))
            {
                query.Append($"action={Uri.EscapeDataString(action)}&");
            }

            if (!string.IsNullOrWhiteSpace(entityType))
            {
                query.Append($"entityType={Uri.EscapeDataString(entityType)}&");
            }

            if (fromDate.HasValue)
            {
                query.Append($"fromDate={fromDate.Value:yyyy-MM-dd}&");
            }

            if (toDate.HasValue)
            {
                query.Append($"toDate={toDate.Value:yyyy-MM-dd}&");
            }

            return query;
        }
    }
}
