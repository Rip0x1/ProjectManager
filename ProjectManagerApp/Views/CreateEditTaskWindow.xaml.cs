using System.Windows;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.ViewModels;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class CreateEditTaskWindow : Window
    {
        public CreateEditTaskWindow()
        {
            InitializeComponent();

            var viewModel = App.GetService<CreateEditTaskViewModel>();
            DataContext = viewModel;

            viewModel.CloseRequested += (_, success) =>
            {
                DialogResult = success;
                Close();
            };
        }

        public async Task PrepareAsync(int? taskId = null, int? projectId = null, string? projectName = null)
        {
            var viewModel = (CreateEditTaskViewModel)DataContext!;

            if (taskId.HasValue)
            {
                await viewModel.InitializeForEdit(taskId.Value);
            }
            else if (projectId.HasValue && !string.IsNullOrEmpty(projectName))
            {
                await viewModel.InitializeForProject(projectId.Value, projectName);
            }
            else if (string.IsNullOrWhiteSpace(viewModel.TaskTitle) && viewModel.Projects.Count == 0)
            {
                await viewModel.LoadAsync();
            }

            WindowFitHelper.ApplySizeBeforeShow(this, 620, 720);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
