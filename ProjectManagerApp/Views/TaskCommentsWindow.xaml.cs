using ProjectManagerApp.ViewModels;
using System.Windows;

namespace ProjectManagerApp.Views
{
    public partial class TaskCommentsWindow : Window
    {
        public TaskCommentsWindow(TaskCommentsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
