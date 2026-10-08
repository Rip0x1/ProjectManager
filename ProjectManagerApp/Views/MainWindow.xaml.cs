using System.Text;
using System.Windows;
using System.Windows.Input;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Services;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.ViewModels;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading.Tasks;
using ProjectManagerApp.Services;
using MaterialDesignThemes.Wpf;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class MainWindow : Window
    {
        private Rect _restoreBounds;
        private bool _isWorkAreaMaximized;

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

        public void MaximizeToWorkArea()
        {
            if (!_isWorkAreaMaximized)
            {
                ApplyWorkAreaMaximize();
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                ApplyWorkAreaMaximize();
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleWorkAreaMaximize();
                return;
            }

            if (_isWorkAreaMaximized)
            {
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
            ToggleWorkAreaMaximize();
        }

        private void ToggleWorkAreaMaximize()
        {
            if (_isWorkAreaMaximized)
            {
                RestoreFromWorkAreaMaximize();
            }
            else
            {
                ApplyWorkAreaMaximize();
            }
        }

        private void ApplyWorkAreaMaximize()
        {
            _restoreBounds = new Rect(Left, Top, Width, Height);
            var workArea = WindowBoundsHelper.GetWorkArea(this);

            Left = workArea.Left;
            Top = workArea.Top;
            Width = workArea.Width;
            Height = workArea.Height;
            _isWorkAreaMaximized = true;

            WindowRootBorder.CornerRadius = new CornerRadius(0);
            MaximizeRestoreIcon.Kind = PackIconKind.WindowRestore;
        }

        private void RestoreFromWorkAreaMaximize()
        {
            if (_restoreBounds.Width > 0 && _restoreBounds.Height > 0)
            {
                Left = _restoreBounds.Left;
                Top = _restoreBounds.Top;
                Width = _restoreBounds.Width;
                Height = _restoreBounds.Height;
            }

            _isWorkAreaMaximized = false;
            WindowRootBorder.CornerRadius = new CornerRadius(8);
            MaximizeRestoreIcon.Kind = PackIconKind.WindowMaximize;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            var auth = App.GetService<IAuthService>();
            await auth.LogoutAsync();
            var nav = App.GetService<INavigationService>();
            nav.NavigateToLogin();
        }

        private sealed partial class MainWindowViewModel : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
        {
            public string CurrentUserNameText { get; }
            public string CurrentUserRoleName { get; }
            public string CurrentUserRoleColor { get; }
            public bool IsAdmin { get; }

            [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
            private bool _isLoading;

            public MainWindowViewModel()
            {
                var auth = App.GetService<IAuthService>();
                CurrentUserNameText = string.IsNullOrWhiteSpace(auth.CurrentUserFirstName)
                    ? auth.CurrentUserLogin
                    : auth.CurrentUserFirstName;

                var role = (UserRole)auth.CurrentUserRole;
                CurrentUserRoleName = role.GetRoleName();
                CurrentUserRoleColor = role.GetRoleColor();
                IsAdmin = role == UserRole.Admin;
            }
        }
    }
}
