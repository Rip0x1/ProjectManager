using System.Windows;
using ProjectManagementSystem.WPF.Models;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class ProjectDetailsWindow : Window
    {
        public ProjectDetailsWindow(ProjectItem project)
        {
            InitializeComponent();
            DataContext = project;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

