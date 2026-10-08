using System.Windows;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.ViewModels;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class ProjectDetailsWindow : Window
    {
        public ProjectDetailsWindow()
        {
            InitializeComponent();
        }

        public async Task PrepareAsync(ProjectItem project)
        {
            var viewModel = App.GetService<ProjectDetailsViewModel>();
            DataContext = viewModel;
            await viewModel.InitializeAsync(project);
            WindowFitHelper.ApplySizeBeforeShow(this, 600, 600);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
