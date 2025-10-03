using System.Threading.Tasks;

namespace ProjectManagementSystem.WPF.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IApiClient _apiClient;

        public StatisticsService(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ProjectStatisticsDto> GetProjectStatisticsAsync(int projectId)
        {
            return await _apiClient.GetAsync<ProjectStatisticsDto>($"Statistics/project/{projectId}");
        }

        public async Task<UserStatisticsDto> GetUserStatisticsAsync(int userId)
        {
            return await _apiClient.GetAsync<UserStatisticsDto>($"Statistics/user/{userId}");
        }

        public async Task<OverviewStatisticsDto> GetOverviewStatisticsAsync()
        {
            return await _apiClient.GetAsync<OverviewStatisticsDto>("Statistics/overview");
        }
    }
}
