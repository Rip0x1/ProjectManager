using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;
using ProjectManagementSystem.WPF.ViewModels;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class UsersView : UserControl
    {
        private readonly Dictionary<string, DataGridColumn> _columnsByKey = new();
        private bool _hasLoaded;

        public UsersView()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                var viewModel = App.GetService<UsersViewModel>();
                DataContext = viewModel;
                RegisterColumns();
                DataGridColumnPickerHelper.Wire(viewModel.ColumnOptions, _columnsByKey);
                IsVisibleChanged += (_, _) => TryLoad(viewModel);
                TryLoad(viewModel);
            }
        }

        private void TryLoad(UsersViewModel viewModel)
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
            _columnsByKey["FullName"] = ColFullName;
            _columnsByKey["Login"] = ColLogin;
            _columnsByKey["Role"] = ColRole;
            _columnsByKey["CreatedAtText"] = ColCreatedAtText;
            _columnsByKey["ManagedProjectsCount"] = ColManagedProjectsCount;
            _columnsByKey["AuthoredTasksCount"] = ColAuthoredTasksCount;
            _columnsByKey["AssignedTasksCount"] = ColAssignedTasksCount;
            _columnsByKey["CommentsCount"] = ColCommentsCount;
            _columnsByKey["Actions"] = ColActions;
        }

        private void ColumnsButton_Click(object sender, RoutedEventArgs e)
        {
            ColumnsPopup.IsOpen = !ColumnsPopup.IsOpen;
        }

        private void UsersGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (UsersGrid.SelectedItem is UserItem user)
            {
                OpenDetails(user);
            }
        }

        private async void OpenDetails(UserItem user)
        {
            var detailsWindow = new UserDetailsWindow
            {
                Owner = Window.GetWindow(this)
            };
            await detailsWindow.PrepareAsync(user);
            detailsWindow.ShowDialog();
        }
    }
}
