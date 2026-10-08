using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProjectManagementSystem.WPF.Helpers
{
    public static class WindowFitHelper
    {
        public static void ApplySizeBeforeShow(Window window, double minWidth = 520, double minHeight = 420)
        {
            window.MinWidth = minWidth;
            window.MinHeight = minHeight;

            var workArea = SystemParameters.WorkArea;
            var maxWidth = workArea.Width * 0.92;
            var maxHeight = workArea.Height * 0.92;

            var scrollViewer = FindScrollViewer(window);
            var contentHeight = 0d;
            var contentWidth = minWidth;
            var chromeHeight = 120d;

            if (window.Content is FrameworkElement root)
            {
                root.Measure(new Size(maxWidth, double.PositiveInfinity));
            }

            if (scrollViewer?.Content is FrameworkElement scrollContent)
            {
                scrollContent.Measure(new Size(maxWidth - 72, double.PositiveInfinity));
                contentHeight = scrollContent.DesiredSize.Height + 32;
                contentWidth = Math.Max(minWidth, scrollContent.DesiredSize.Width + 72);
                chromeHeight = GetChromeHeight(scrollViewer);
            }
            else if (window.Content is FrameworkElement measuredRoot)
            {
                contentHeight = measuredRoot.DesiredSize.Height;
                contentWidth = Math.Max(minWidth, measuredRoot.DesiredSize.Width + 48);
                chromeHeight = 48;
            }

            var targetWidth = Math.Max(minWidth, Math.Min(contentWidth, maxWidth));
            var targetHeight = Math.Max(minHeight, Math.Min(chromeHeight + contentHeight, maxHeight));

            window.Width = targetWidth;
            window.Height = targetHeight;
            CenterWindow(window, targetWidth, targetHeight, workArea);
        }

        public static bool? ShowDialogSized(Window window, double minWidth = 520, double minHeight = 420)
        {
            ApplySizeBeforeShow(window, minWidth, minHeight);
            return window.ShowDialog();
        }

        private static double GetChromeHeight(ScrollViewer scrollViewer)
        {
            if (scrollViewer.Parent is not Grid grid)
            {
                return 120;
            }

            var chrome = 0d;
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(grid); i++)
            {
                if (VisualTreeHelper.GetChild(grid, i) is not FrameworkElement child || ReferenceEquals(child, scrollViewer))
                {
                    continue;
                }

                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                chrome += child.DesiredSize.Height;
            }

            return chrome + grid.Margin.Top + grid.Margin.Bottom + 24;
        }

        private static ScrollViewer? FindScrollViewer(DependencyObject parent)
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is ScrollViewer scrollViewer)
                {
                    return scrollViewer;
                }

                var nested = FindScrollViewer(child);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
        }

        private static void CenterWindow(Window window, double width, double height, Rect workArea)
        {
            if (window.WindowStartupLocation == WindowStartupLocation.CenterOwner && window.Owner != null)
            {
                window.Left = window.Owner.Left + (window.Owner.Width - width) / 2;
                window.Top = window.Owner.Top + (window.Owner.Height - height) / 2;
            }
            else
            {
                window.Left = workArea.Left + (workArea.Width - width) / 2;
                window.Top = workArea.Top + (workArea.Height - height) / 2;
            }
        }
    }
}
