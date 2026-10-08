using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.ViewModels;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class TasksView : UserControl
    {
        private readonly Dictionary<string, DataGridColumn> _columnsByKey = new();
        private bool _hasLoaded;

        public TasksView()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = App.GetService<TasksViewModel>();
            }

            InitializeComponent();

            RegisterColumns();

            if (DataContext is TasksViewModel viewModel)
            {
                DataGridColumnPickerHelper.Wire(viewModel.ColumnOptions, _columnsByKey);
                IsVisibleChanged += (_, _) => TryLoad(viewModel);
                TryLoad(viewModel);
            }
        }

        private void TryLoad(TasksViewModel viewModel)
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
            _columnsByKey["Title"] = ColTitle;
            _columnsByKey["Description"] = ColDescription;
            _columnsByKey["Status"] = ColStatus;
            _columnsByKey["Priority"] = ColPriority;
            _columnsByKey["ProjectName"] = ColProjectName;
            _columnsByKey["AssigneeDisplayName"] = ColAssigneeDisplayName;
            _columnsByKey["DueDateText"] = ColDueDateText;
            _columnsByKey["CompletedAtText"] = ColCompletedAtText;
            _columnsByKey["AuthorName"] = ColAuthorName;
            _columnsByKey["CreatedAtText"] = ColCreatedAtText;
            _columnsByKey["Actions"] = ColActions;
        }

        private void ColumnsButton_Click(object sender, RoutedEventArgs e)
        {
            ColumnsPopup.IsOpen = !ColumnsPopup.IsOpen;
        }

        private void TasksGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (TasksGrid.SelectedItem is TaskItem task)
            {
                OpenDetails(task);
            }
        }

        private void TasksGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            if (DataContext is TasksViewModel viewModel)
            {
                e.Handled = true;
                var direction = e.Column.SortDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
                e.Column.SortDirection = direction;
                viewModel.SetSort(e.Column.SortMemberPath, direction);
            }
        }

        private async void OpenDetails(TaskItem task)
        {
            var detailsWindow = new TaskDetailsWindow
            {
                Owner = Window.GetWindow(this)
            };

            await detailsWindow.PrepareAsync(task);
            detailsWindow.ShowDialog();
        }
    }
}
