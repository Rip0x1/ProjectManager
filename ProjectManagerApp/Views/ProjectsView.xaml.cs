using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ProjectManagementSystem.WPF;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.ViewModels;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class ProjectsView : UserControl
    {
        private readonly Dictionary<string, DataGridColumn> _columnsByKey = new();
        private bool _hasLoaded;

        public ProjectsView()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            var vm = App.GetService<ProjectsViewModel>();
            DataContext = vm;
            RegisterColumns();
            WireColumnVisibility(vm);
            IsVisibleChanged += (_, _) => TryLoad(vm);
            TryLoad(vm);
        }

        private void TryLoad(ProjectsViewModel viewModel)
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
            _columnsByKey["CreatedAtText"] = ColCreatedAtText;
            _columnsByKey["TasksCount"] = ColTasksCount;
            _columnsByKey["ParticipantsCount"] = ColParticipantsCount;
            _columnsByKey["Actions"] = ColActions;
        }

        private void WireColumnVisibility(ProjectsViewModel viewModel)
        {
            DataGridColumnPickerHelper.Wire(viewModel.ColumnOptions, _columnsByKey);
        }

        private void ColumnsButton_Click(object sender, RoutedEventArgs e)
        {
            ColumnsPopup.IsOpen = !ColumnsPopup.IsOpen;
        }

        private void ProjectsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ProjectsGrid.SelectedItem is ProjectItem project)
            {
                OpenDetails(project);
            }
        }

        private async void OpenDetails(ProjectItem project)
        {
            var detailsWindow = new ProjectDetailsWindow
            {
                Owner = Window.GetWindow(this)
            };
            await detailsWindow.PrepareAsync(project);
            detailsWindow.ShowDialog();
        }
    }
}
