using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ProjectManagementSystem.WPF.Helpers
{
    public static class WindowBoundsHelper
    {
        private const uint MonitorDefaultToNearest = 2;

        public static Rect GetWorkArea(Window window)
        {
            var handle = new WindowInteropHelper(window).Handle;
            if (handle == IntPtr.Zero)
            {
                return SystemParameters.WorkArea;
            }

            var monitor = MonitorFromWindow(handle, MonitorDefaultToNearest);
            var info = new MonitorInfo
            {
                Size = Marshal.SizeOf<MonitorInfo>()
            };

            if (!GetMonitorInfo(monitor, ref info))
            {
                return SystemParameters.WorkArea;
            }

            return new Rect(
                info.WorkArea.Left,
                info.WorkArea.Top,
                info.WorkArea.Right - info.WorkArea.Left,
                info.WorkArea.Bottom - info.WorkArea.Top);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MonitorInfo
        {
            public int Size;
            public RectNative MonitorArea;
            public RectNative WorkArea;
            public uint Flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RectNative
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
