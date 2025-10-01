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

namespace ProjectManagementSystem.WPF.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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

        private sealed class MainWindowViewModel
        {
            public string CurrentUserNameText { get; }
            public string CurrentUserRoleName { get; }
            public string CurrentUserRoleColor { get; }

            public MainWindowViewModel()
            {
                var auth = App.GetService<IAuthService>();
                CurrentUserNameText = string.IsNullOrWhiteSpace(auth.CurrentUserFirstName)
                    ? auth.CurrentUserEmail
                    : auth.CurrentUserFirstName;
                
                var role = (UserRole)auth.CurrentUserRole;
                CurrentUserRoleName = role.GetRoleName();
                CurrentUserRoleColor = role.GetRoleColor();
            }
        }
    }
}