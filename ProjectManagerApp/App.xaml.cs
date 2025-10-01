using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProjectManagementSystem.WPF.Services;
using ProjectManagementSystem.WPF.ViewModels;
using ProjectManagementSystem.WPF.Views;
using System.Windows;
using NavigationService = ProjectManagementSystem.WPF.Services.NavigationService;

namespace ProjectManagementSystem.WPF
{
    public partial class App : Application
    {
        private static IHost? Host { get; set; }

        public App()
        {
            Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureServices(ConfigureServices)
                .Build();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IApiClient, ApiClient>();
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IPermissionService, PermissionService>();
            services.AddSingleton<IProjectsService, ProjectsService>();
            services.AddSingleton<ITasksService, TasksService>();
            services.AddSingleton<IUsersService, UsersService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddSingleton<ILoginNotificationService, LoginNotificationService>();

            services.AddTransient<LoginViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<ProjectsViewModel>();
            services.AddTransient<TasksViewModel>();
            services.AddTransient<UsersViewModel>();
            services.AddTransient<CreateEditProjectViewModel>();
            services.AddTransient<CreateEditTaskViewModel>();

            services.AddTransient<LoginView>();
            services.AddSingleton<MainWindow>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await Host!.StartAsync();

            var mainWindow = Host.Services.GetRequiredService<MainWindow>();
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (Host)
            {
                await Host!.StopAsync();
            }
            base.OnExit(e);
        }

        public static T GetService<T>() where T : class
        {
            return Host!.Services.GetService<T>()!;
        }
    }
}