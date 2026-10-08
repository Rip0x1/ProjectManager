using System.Windows;
using System.Windows.Media.Imaging;
using ProjectManagementSystem.WPF.Helpers;

namespace ProjectManagementSystem.WPF.Views
{
    public partial class AttachmentPreviewWindow : Window
    {
        private const double HorizontalChrome = 64;
        private const double VerticalChrome = 132;

        private byte[] _fileBytes = Array.Empty<byte>();
        private string _fileName = string.Empty;

        public AttachmentPreviewWindow()
        {
            InitializeComponent();
        }

        public void LoadContent(string fileName, byte[] bytes, AttachmentPreviewType previewType)
        {
            _fileName = fileName;
            _fileBytes = bytes;
            TitleText.Text = fileName;
            Title = $"Предпросмотр — {fileName}";

            ImageViewbox.Visibility = Visibility.Collapsed;
            TextScroll.Visibility = Visibility.Collapsed;
            UnsupportedPanel.Visibility = Visibility.Collapsed;

            switch (previewType)
            {
                case AttachmentPreviewType.Image:
                    var image = AttachmentPreviewHelper.CreateImageSource(bytes);
                    PreviewImage.Source = image;
                    ImageViewbox.Visibility = Visibility.Visible;
                    FitImageWindow(image);
                    break;
                case AttachmentPreviewType.Text:
                    PreviewText.Text = AttachmentPreviewHelper.ReadTextContent(bytes);
                    TextScroll.Visibility = Visibility.Visible;
                    Width = 720;
                    Height = 520;
                    break;
                default:
                    UnsupportedPanel.Visibility = Visibility.Visible;
                    Width = 560;
                    Height = 360;
                    break;
            }
        }

        private void FitImageWindow(BitmapImage image)
        {
            var workArea = SystemParameters.WorkArea;
            var maxImageWidth = workArea.Width * 0.92 - HorizontalChrome;
            var maxImageHeight = workArea.Height * 0.92 - VerticalChrome;

            var imageWidth = image.PixelWidth;
            var imageHeight = image.PixelHeight;
            if (imageWidth < 1 || imageHeight < 1)
            {
                Width = 720;
                Height = 520;
                return;
            }

            var scale = Math.Min(maxImageWidth / imageWidth, maxImageHeight / imageHeight);
            var displayWidth = imageWidth * scale;
            var displayHeight = imageHeight * scale;

            PreviewImage.Width = imageWidth;
            PreviewImage.Height = imageHeight;

            Width = Math.Max(MinWidth, displayWidth + HorizontalChrome);
            Height = Math.Max(MinHeight, displayHeight + VerticalChrome);

            if (Width > workArea.Width * 0.98)
            {
                Width = workArea.Width * 0.98;
            }

            if (Height > workArea.Height * 0.98)
            {
                Height = workArea.Height * 0.98;
            }

            CenterOnScreen(workArea);
        }

        private void CenterOnScreen(Rect workArea)
        {
            if (WindowStartupLocation == WindowStartupLocation.CenterOwner && Owner != null)
            {
                Left = Owner.Left + (Owner.Width - Width) / 2;
                Top = Owner.Top + (Owner.Height - Height) / 2;
                return;
            }

            Left = workArea.Left + (workArea.Width - Width) / 2;
            Top = workArea.Top + (workArea.Height - Height) / 2;
        }

        private async void OpenExternalButton_Click(object sender, RoutedEventArgs e)
        {
            await AttachmentPreviewHelper.OpenExternallyAsync(_fileName, _fileBytes);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
