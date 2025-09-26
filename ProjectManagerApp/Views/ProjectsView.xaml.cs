using System.Windows.Controls;
using System.ComponentModel;
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
    }
}

