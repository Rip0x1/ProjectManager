using System.Text;
using System.Windows;
using System.Windows.Input;
using ProjectManagementSystem.WPF.Services;
using ProjectManagementSystem.WPF.Models;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading.Tasks;
using ProjectManagerApp.Services;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            
            var notificationService = App.GetService<INotificationService>();
            
            if (notificationService.MessageQueue == null)
            {
                notificationService.MessageQueue = new MaterialDesignThemes.Wpf.SnackbarMessageQueue(TimeSpan.FromSeconds(3));
            }
            
            MainSnackbar.MessageQueue = notificationService.MessageQueue;
            notificationService.SetNotificationBorder(NotificationBorder);
            
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var tabControl = FindName("TabControl") as TabControl;
            if (tabControl != null)
            {
                foreach (TabItem tab in tabControl.Items)
                {
                    if (tab.Content is ProjectManagerApp.Views.MyProjectsView myProjectsView)
                    {
                        var viewModel = App.GetService<ProjectManagerApp.ViewModels.MyProjectsViewModel>();
                        myProjectsView.DataContext = viewModel;
                    }
                }
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                return;
            }
            DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeRestore_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var auth = App.GetService<IAuthService>();
            auth.Logout();
            var nav = App.GetService<INavigationService>();
            nav.NavigateToLogin();
        }

        private sealed partial class MainWindowViewModel : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
        {
            public string CurrentUserNameText { get; }
            public string CurrentUserRoleName { get; }
            public string CurrentUserRoleColor { get; }

            [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
            private bool _isLoading = true;

            public MainWindowViewModel()
            {
                var auth = App.GetService<IAuthService>();
                CurrentUserNameText = string.IsNullOrWhiteSpace(auth.CurrentUserFirstName)
                    ? auth.CurrentUserEmail
                    : auth.CurrentUserFirstName;
                
                var role = (UserRole)auth.CurrentUserRole;
                CurrentUserRoleName = role.GetRoleName();
                CurrentUserRoleColor = role.GetRoleColor();

            // Запускаем реальную загрузку данных
            _ = Task.Run(async () =>
            {
                try
                {
                    // Загружаем все необходимые данные
                    var apiClient = App.GetService<IApiClient>();
                    var authService = App.GetService<IAuthService>();
                    
                    // Загружаем статистику
                    await apiClient.GetAsync<object>("statistics/overview");
                    
                    // Загружаем проекты
                    await apiClient.GetAsync<object>("projects");
                    
                    // Загружаем задачи
                    await apiClient.GetAsync<object>("tasks");
                    
                    // Загружаем пользователей
                    await apiClient.GetAsync<object>("users");
                    
                    // Загружаем проекты пользователя
                    await apiClient.GetAsync<object>($"projects/user/{authService.CurrentUserId}");
                }
                catch (Exception ex)
                {
                    // Логируем ошибку, но не показываем пользователю
                    System.Diagnostics.Debug.WriteLine($"Ошибка загрузки данных: {ex.Message}");
                }
                finally
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        IsLoading = false;
                    });
                }
            });
            }
        }
    }
}