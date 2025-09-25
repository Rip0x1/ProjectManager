using ProjectManagementSystem.WPF.Views;
using ProjectManagerApp;
using System.Windows;

namespace ProjectManagementSystem.WPF.Services
{
    public class NavigationService : INavigationService
    {
        public void NavigateToLogin()
        {
            var loginWindow = new LoginView();
            loginWindow.Show();
            Application.Current.MainWindow?.Close();
            Application.Current.MainWindow = loginWindow;
        }

        public void NavigateToDashboard()
        {
            var oldWindow = Application.Current.MainWindow;
            var mainWindow = new MainWindow();
            Application.Current.MainWindow = mainWindow;
            mainWindow.WindowState = WindowState.Maximized;
            mainWindow.Show();
            if (oldWindow != null && oldWindow != mainWindow)
            {
                oldWindow.Close();
            }
            foreach (Window w in Application.Current.Windows)
            {
                if (w != mainWindow)
                {
                    w.Close();
                }
            }
        }
    }
}