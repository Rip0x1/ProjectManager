using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.ViewModels;
using ProjectManagementSystem.WPF;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class ProjectsView : UserControl
    {
        public ProjectsView()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
            var vm = App.GetService<ProjectsViewModel>();
            DataContext = vm;
            _ = vm.LoadAsync();
        }

        private void ProjectCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is ProjectItem project)
            {
                var detailsWindow = new ProjectDetailsWindow(project);
                detailsWindow.Owner = Window.GetWindow(this);
                detailsWindow.ShowDialog();
            }
        }
    }
}

