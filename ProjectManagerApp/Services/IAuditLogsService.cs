using ProjectManagementSystem.WPF.Models;

namespace ProjectManagementSystem.WPF.Services
{
    public interface IAuditLogsService
    {
        Task<PagedAuditLogsDto> GetLogsAsync(
            string? searchTerm,
            string? action,
            string? entityType,
            DateTime? fromDate,
            DateTime? toDate,
            int pageNumber,
            int pageSize);

        Task<int> ClearLogsAsync(
            string? searchTerm,
            string? action,
            string? entityType,
            DateTime? fromDate,
            DateTime? toDate);
    }
}
