using System.IO;

namespace ProjectManagementSystem.WPF.Helpers
{
    public static class AttachmentInputHelper
    {
        public static bool TryValidateFiles(
            IEnumerable<string> filePaths,
            int existingCount,
            out string? errorMessage)
        {
            errorMessage = null;
            var paths = filePaths.ToList();

            if (paths.Count == 0)
            {
                return true;
            }

            if (existingCount + paths.Count > ValidationLimits.MaxAttachmentsPerSelection)
            {
                errorMessage = $"Можно прикрепить не более {ValidationLimits.MaxAttachmentsPerSelection} файлов за раз.";
                return false;
            }

            foreach (var path in paths)
            {
                if (!File.Exists(path))
                {
                    errorMessage = "Выбранный файл не найден.";
                    return false;
                }

                var info = new FileInfo(path);
                if (info.Length > ValidationLimits.MaxAttachmentSizeBytes)
                {
                    errorMessage = $"Файл «{info.Name}» слишком большой. Максимум — {ValidationLimits.MaxAttachmentSizeBytes / (1024 * 1024)} МБ.";
                    return false;
                }

                if (info.Length == 0)
                {
                    errorMessage = $"Файл «{info.Name}» пустой.";
                    return false;
                }
            }

            return true;
        }
    }
}
