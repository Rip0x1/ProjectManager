using CommunityToolkit.Mvvm.ComponentModel;
using ProjectManagementSystem.WPF.Services;

namespace ProjectManagementSystem.WPF.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        public string RoleName => _authService.CurrentUserRole;

        public DashboardViewModel(IAuthService authService)
        {
            _authService = authService;
        }
    }
}
