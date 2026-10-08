using System.Windows;
using ProjectManagementSystem.WPF.Views;

namespace ProjectManagementSystem.WPF.Helpers
{
    public static class PermissionMatrixHelper
    {
        public static void Show(Window? owner = null)
        {
            var window = new PermissionsMatrixWindow
            {
                Owner = owner ?? Application.Current.MainWindow
            };
            window.ShowDialog();
        }
    }
}
