using System.Windows;

namespace ProjectManagerApp.Views
{
    public partial class ProjectMembersViewWindow : Window
    {
        public ProjectMembersViewWindow()
        {
            InitializeComponent();
            
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
