using System.Windows;
using System.Windows.Controls;

namespace ProjectManagementSystem.WPF.Helpers
{
    public static class DataGridHelper
    {
        public static readonly DependencyProperty AutoCenterTextColumnsProperty =
            DependencyProperty.RegisterAttached(
                "AutoCenterTextColumns",
                typeof(bool),
                typeof(DataGridHelper),
                new PropertyMetadata(false, OnAutoCenterTextColumnsChanged));

        public static bool GetAutoCenterTextColumns(DependencyObject element)
        {
            return (bool)element.GetValue(AutoCenterTextColumnsProperty);
        }

        public static void SetAutoCenterTextColumns(DependencyObject element, bool value)
        {
            element.SetValue(AutoCenterTextColumnsProperty, value);
        }

        private static void OnAutoCenterTextColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid grid || e.NewValue is not true)
            {
                return;
            }

            if (grid.IsLoaded)
            {
                ApplyTextColumnStyles(grid);
            }
            else
            {
                grid.Loaded += (_, _) => ApplyTextColumnStyles(grid);
            }
        }

        public static void ApplyTextColumnStyles(DataGrid grid)
        {
            var textStyle = grid.TryFindResource("DataGridCellTextStyle") as Style
                            ?? Application.Current.TryFindResource("DataGridCellTextStyle") as Style;

            if (textStyle is null)
            {
                return;
            }

            foreach (var column in grid.Columns)
            {
                if (column is DataGridTextColumn textColumn
                    && textColumn.ReadLocalValue(DataGridTextColumn.ElementStyleProperty) == DependencyProperty.UnsetValue)
                {
                    textColumn.ElementStyle = textStyle;
                    textColumn.EditingElementStyle = textStyle;
                }
            }
        }
    }
}
