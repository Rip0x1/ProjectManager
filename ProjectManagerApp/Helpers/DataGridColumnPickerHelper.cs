using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ProjectManagementSystem.WPF.Models;

namespace ProjectManagementSystem.WPF.Helpers
{
    public static class DataGridColumnPickerHelper
    {
        public static void Wire(
            IEnumerable<ColumnVisibilityOption> options,
            IReadOnlyDictionary<string, DataGridColumn> columns)
        {
            foreach (var option in options)
            {
                option.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(ColumnVisibilityOption.IsVisible))
                    {
                        Apply(options, columns);
                    }
                };
            }

            Apply(options, columns);
        }

        public static void Apply(
            IEnumerable<ColumnVisibilityOption> options,
            IReadOnlyDictionary<string, DataGridColumn> columns)
        {
            foreach (var option in options)
            {
                if (columns.TryGetValue(option.Key, out var column))
                {
                    column.Visibility = option.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }
    }
}
