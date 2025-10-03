using System.Windows;
using System.Windows.Input;

namespace ProjectManagerApp.Views
{
    public partial class ProjectCommentsViewWindow : Window
    {
        public ProjectCommentsViewWindow()
        {
            InitializeComponent();
        }

        private void DragWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
