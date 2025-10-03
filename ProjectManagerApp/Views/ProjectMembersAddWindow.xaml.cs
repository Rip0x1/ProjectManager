using System.Windows;

namespace ProjectManagerApp.Views
{
    public partial class ProjectMembersAddWindow : Window
    {
        public ProjectMembersAddWindow()
        {
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
