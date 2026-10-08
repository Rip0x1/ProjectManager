using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ProjectManagerApp.Models;
using ProjectManagerApp.ViewModels;
using ProjectManagementSystem.WPF;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Views;

namespace ProjectManagerApp.Views
{
    public partial class MyProjectsView : UserControl
    {
        private readonly Dictionary<string, DataGridColumn> _columnsByKey = new();
        private bool _hasLoaded;

        public MyProjectsView()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                var viewModel = App.GetService<MyProjectsViewModel>();
                DataContext = viewModel;
                RegisterColumns();
                DataGridColumnPickerHelper.Wire(viewModel.ColumnOptions, _columnsByKey);
                IsVisibleChanged += (_, _) => TryLoad(viewModel);
                TryLoad(viewModel);
            }
        }

        private void TryLoad(MyProjectsViewModel viewModel)
        {
            if (_hasLoaded || !IsVisible)
            {
                return;
            }

            _hasLoaded = true;
            _ = viewModel.LoadAsync();
        }

        private void RegisterColumns()
        {
            _columnsByKey["Id"] = ColId;
            _columnsByKey["Name"] = ColName;
            _columnsByKey["Status"] = ColStatus;
            _columnsByKey["ManagerName"] = ColManagerName;
            _columnsByKey["DeadlineText"] = ColDeadlineText;
            _columnsByKey["ProgressText"] = ColProgressText;
            _columnsByKey["ParticipantsCount"] = ColParticipantsCount;
            _columnsByKey["TasksCount"] = ColTasksCount;
            _columnsByKey["CommentsCount"] = ColCommentsCount;
            _columnsByKey["Actions"] = ColActions;
        }

        private void ColumnsButton_Click(object sender, RoutedEventArgs e)
        {
            ColumnsPopup.IsOpen = !ColumnsPopup.IsOpen;
        }

        private void MyProjectsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (MyProjectsGrid.SelectedItem is UserProjectItem project)
            {
                OpenDetails(project);
            }
        }

        private async void OpenDetails(UserProjectItem project)
        {
            var detailsWindow = new ProjectDetailsWindow
            {
                Owner = Window.GetWindow(this)
            };
            await detailsWindow.PrepareAsync(ToProjectItem(project));
            detailsWindow.ShowDialog();
        }

        private static ProjectItem ToProjectItem(UserProjectItem project) => new()
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            ManagerId = project.ManagerId,
            CreatedAt = project.CreatedAt,
            Deadline = project.Deadline,
            ManagerName = project.ManagerName,
            Status = project.Status,
            ParticipantsCount = project.ParticipantsCount,
            TasksCount = project.TasksCount,
            CommentsCount = project.CommentsCount
        };
    }
}
