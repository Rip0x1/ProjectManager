using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ProjectManagementSystem.WPF.Helpers;
using ProjectManagementSystem.WPF.Models;
using ProjectManagementSystem.WPF.Services;
using ProjectManagementSystem.WPF.ViewModels;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class AuditLogsView : UserControl
    {
        private readonly Dictionary<string, DataGridColumn> _columnsByKey = new();
        private bool _isDetailsOpen;

        public AuditLogsView()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                var viewModel = App.GetService<AuditLogsViewModel>();
                DataContext = viewModel;
                RegisterColumns();
                DataGridColumnPickerHelper.Wire(viewModel.ColumnOptions, _columnsByKey);
                IsVisibleChanged += AuditLogsView_IsVisibleChanged;
                if (IsVisible)
                {
                    _ = viewModel.LoadAsync();
                }
            }
        }

        private void AuditLogsView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible && DataContext is AuditLogsViewModel viewModel)
            {
                _ = viewModel.LoadAsync();
            }
        }

        private void RegisterColumns()
        {
            _columnsByKey["Id"] = ColId;
            _columnsByKey["TimestampText"] = ColTimestampText;
            _columnsByKey["UserDisplay"] = ColUserDisplay;
            _columnsByKey["ActionText"] = ColActionText;
            _columnsByKey["EntityTypeText"] = ColEntityTypeText;
            _columnsByKey["EntityIdText"] = ColEntityIdText;
            _columnsByKey["EntityName"] = ColEntityName;
            _columnsByKey["Details"] = ColDetails;
        }

        private void ColumnsButton_Click(object sender, RoutedEventArgs e)
        {
            ColumnsPopup.IsOpen = !ColumnsPopup.IsOpen;
        }

        private void LogsGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left || _isDetailsOpen)
            {
                return;
            }

            if (e.OriginalSource is not DependencyObject source)
            {
                return;
            }

            if (FindParent<DataGridColumnHeader>(source) != null || FindParent<ScrollBar>(source) != null)
            {
                return;
            }

            var row = FindParent<DataGridRow>(source);
            if (row?.Item is not AuditLogItem log)
            {
                return;
            }

            LogsGrid.SelectedItem = log;
            e.Handled = true;
            OpenDetails(log);
        }

        private void OpenDetails(AuditLogItem log)
        {
            if (_isDetailsOpen)
            {
                return;
            }

            _isDetailsOpen = true;
            Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    var window = new AuditLogDetailsWindow(log)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    window.ShowDialog();
                }
                finally
                {
                    _isDetailsOpen = false;
                }
            }, DispatcherPriority.Background);
        }

        private static T? FindParent<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T match)
                {
                    return match;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }
    }
}
