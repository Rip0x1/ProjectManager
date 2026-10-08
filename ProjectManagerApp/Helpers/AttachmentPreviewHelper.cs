using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using ProjectManagementSystem.WPF.Views;

namespace ProjectManagementSystem.WPF.Helpers
{
    public static class AttachmentPreviewHelper
    {
        private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp"
        };

        private static readonly HashSet<string> TextExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".txt", ".csv", ".json", ".xml", ".log", ".md"
        };

        public static bool CanPreviewInApp(string fileName, string? contentType)
        {
            var extension = Path.GetExtension(fileName);
            if (ImageExtensions.Contains(extension) || TextExtensions.Contains(extension))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(contentType))
            {
                return contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
                    || contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public static void ShowPreview(Window owner, string fileName, byte[] bytes, string? contentType)
        {
            var extension = Path.GetExtension(fileName);
            var previewType = ResolvePreviewType(fileName, contentType, extension);

            var window = new AttachmentPreviewWindow
            {
                Owner = owner
            };
            window.LoadContent(fileName, bytes, previewType);
            window.ShowDialog();
        }

        public static async Task OpenExternallyAsync(string fileName, byte[] bytes)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "ProjectManager", Guid.NewGuid().ToString("N") + Path.GetExtension(fileName));
            Directory.CreateDirectory(Path.GetDirectoryName(tempPath)!);
            await File.WriteAllBytesAsync(tempPath, bytes);
            Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
        }

        public static BitmapImage CreateImageSource(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = stream;

            var workArea = SystemParameters.WorkArea;
            var maxDecode = (int)Math.Max(workArea.Width, workArea.Height) * 2;
            if (maxDecode > 0)
            {
                image.DecodePixelWidth = maxDecode;
            }

            image.EndInit();
            image.Freeze();
            return image;
        }

        public static string ReadTextContent(byte[] bytes)
        {
            try
            {
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return Encoding.Default.GetString(bytes);
            }
        }

        private static AttachmentPreviewType ResolvePreviewType(string fileName, string? contentType, string extension)
        {
            if (ImageExtensions.Contains(extension) || contentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true)
            {
                return AttachmentPreviewType.Image;
            }

            if (TextExtensions.Contains(extension) || contentType?.StartsWith("text/", StringComparison.OrdinalIgnoreCase) == true)
            {
                return AttachmentPreviewType.Text;
            }

            return AttachmentPreviewType.Unsupported;
        }
    }

    public enum AttachmentPreviewType
    {
        Image,
        Text,
        Unsupported
    }
}
