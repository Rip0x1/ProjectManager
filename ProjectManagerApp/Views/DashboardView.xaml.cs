using System.ComponentModel;
using System.Windows.Controls;
using ProjectManagementSystem.WPF.ViewModels;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                var viewModel = App.GetService<DashboardViewModel>();
                DataContext = viewModel;
                _ = viewModel.LoadDataAsync();
            }
        }
    }
}

