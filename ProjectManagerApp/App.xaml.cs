using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProjectManagementSystem.WPF.Services;
using ProjectManagementSystem.WPF.ViewModels;
using ProjectManagementSystem.WPF.Views;
using ProjectManagerApp.Services;
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
            services.AddSingleton<ICommentsService, CommentsService>();
            services.AddSingleton<IProjectMembersService, ProjectMembersService>();
            services.AddSingleton<IUserProjectsService, UserProjectsService>();
            services.AddSingleton<IStatisticsService, StatisticsService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<INotificationService, NotificationService>();
            services.AddSingleton<ILoginNotificationService, LoginNotificationService>();

            services.AddTransient<LoginViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<ProjectsViewModel>();
            services.AddTransient<ProjectManagerApp.ViewModels.MyProjectsViewModel>();
            services.AddTransient<TasksViewModel>();
            services.AddTransient<UsersViewModel>();
            services.AddTransient<CreateEditProjectViewModel>();
            services.AddTransient<CreateEditTaskViewModel>();
            services.AddTransient<ProjectManagerApp.ViewModels.ProjectMembersViewViewModel>();
            services.AddTransient<ProjectManagerApp.ViewModels.ProjectMembersAddViewModel>();
            services.AddTransient<ProjectManagerApp.ViewModels.ProjectCommentViewModel>();
            services.AddTransient<ProjectManagerApp.ViewModels.ProjectCommentsViewViewModel>();
            services.AddTransient<ProjectManagerApp.ViewModels.ProjectCommentsAddViewModel>();
            services.AddTransient<ProjectManagerApp.ViewModels.ProjectTasksViewModel>();

            services.AddTransient<LoginView>();
            services.AddSingleton<MainWindow>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await Host!.StartAsync();

            // Показываем окно авторизации
            var loginView = Host.Services.GetRequiredService<LoginView>();
            loginView.Show();

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

        public static IServiceProvider ServiceProvider => Host!.Services;
    }
}