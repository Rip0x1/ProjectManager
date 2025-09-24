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
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Application.Current.MainWindow?.Close();
            Application.Current.MainWindow = mainWindow;
        }
    }
}