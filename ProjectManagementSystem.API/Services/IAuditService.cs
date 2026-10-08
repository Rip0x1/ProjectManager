using ProjectManagementSystem.API.Models.DTOs;

namespace ProjectManagementSystem.API.Services
{
    public interface IAuditService
    {
        Task LogAsync(string action, string entityType, int? entityId, string entityName,
                     int? userId, string userLogin, string userName, string? oldValues, string? newValues,
                     string ipAddress, string userAgent, string details = "");

        Task<PagedAuditLogsDto> GetLogsAsync(AuditLogFilterDto filter);

        Task<int> DeleteLogsAsync(AuditLogFilterDto filter);
    }
}
