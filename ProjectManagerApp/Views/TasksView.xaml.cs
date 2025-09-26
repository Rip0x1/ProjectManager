using System.ComponentModel;
using System.Windows.Controls;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class TasksView : UserControl
    {
        public TasksView()
        {
            InitializeComponent();
            
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = App.GetService<ViewModels.TasksViewModel>();
            }
        }
    }
}
