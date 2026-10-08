using System.Text;
using System.Text.RegularExpressions;

namespace ProjectManagementSystem.WPF.Helpers
{
    public static class InputValidation
    {
        private static readonly Regex PersonNameCharRegex = new(@"^[\p{L}\s\-']$", RegexOptions.Compiled);
        private static readonly Regex LoginCharRegex = new(@"^[a-zA-Z0-9._\-]$", RegexOptions.Compiled);
        private static readonly Regex LoginValueRegex = new(@"^[a-zA-Z0-9._-]{3,100}$", RegexOptions.Compiled);
        private static readonly Regex PageNumberRegex = new(@"^\d+$", RegexOptions.Compiled);

        public static bool IsAllowedPersonNameChar(char c) =>
            PersonNameCharRegex.IsMatch(c.ToString());

        public static bool IsAllowedLoginChar(char c) =>
            LoginCharRegex.IsMatch(c.ToString());

        public static bool IsAllowedSearchChar(char c) =>
            !char.IsControl(c) || c == '\t';

        public static bool IsAllowedTextChar(char c, bool allowNewLine) =>
            allowNewLine
                ? c == '\r' || c == '\n' || !char.IsControl(c)
                : !char.IsControl(c);

        public static bool IsAllowedPasswordChar(char c) => !char.IsControl(c);

        public static bool IsAllowedPageNumberChar(char c) => char.IsDigit(c);

        public static string SanitizePersonName(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            foreach (var c in value)
            {
                if (IsAllowedPersonNameChar(c))
                {
                    builder.Append(c);
                }
            }

            return Truncate(builder.ToString(), ValidationLimits.PersonNameMax);
        }

        public static string SanitizeLogin(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            foreach (var c in value)
            {
                if (IsAllowedLoginChar(c))
                {
                    builder.Append(c);
                }
            }

            return Truncate(builder.ToString(), ValidationLimits.LoginMax);
        }

        public static string SanitizePassword(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            foreach (var c in value)
            {
                if (IsAllowedPasswordChar(c))
                {
                    builder.Append(c);
                }
            }

            return Truncate(builder.ToString(), ValidationLimits.PasswordMax);
        }

        public static string SanitizeSearch(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            foreach (var c in value)
            {
                if (IsAllowedSearchChar(c))
                {
                    builder.Append(c);
                }
            }

            return Truncate(builder.ToString(), ValidationLimits.SearchMax);
        }

        public static string SanitizeTitle(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            foreach (var c in value)
            {
                if (IsAllowedTextChar(c, allowNewLine: false))
                {
                    builder.Append(c);
                }
            }

            return Truncate(builder.ToString(), maxLength);
        }

        public static string SanitizeMultilineText(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            foreach (var c in value)
            {
                if (IsAllowedTextChar(c, allowNewLine: true))
                {
                    builder.Append(c);
                }
            }

            return Truncate(builder.ToString(), maxLength);
        }

        public static string SanitizePageNumber(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var digits = new string(value.Where(char.IsDigit).ToArray());
            if (digits.Length > 6)
            {
                digits = digits[..6];
            }

            return digits;
        }

        public static int ClampPage(int page, int totalPages)
        {
            if (totalPages <= 0)
            {
                return 1;
            }

            if (page < 1)
            {
                return 1;
            }

            if (page > totalPages)
            {
                return totalPages;
            }

            return page;
        }

        public static bool TryParsePage(string? value, out int page)
        {
            page = 1;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var sanitized = SanitizePageNumber(value);
            return int.TryParse(sanitized, out page) && page >= 1;
        }

        public static bool IsValidLogin(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                return false;
            }

            var trimmed = login.Trim();
            if (trimmed.Length < ValidationLimits.LoginMin || trimmed.Length > ValidationLimits.LoginMax)
            {
                return false;
            }

            return LoginValueRegex.IsMatch(trimmed);
        }

        public static List<string> ValidatePersonName(string? value, string fieldLabel)
        {
            var errors = new List<string>();
            var trimmed = value?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(trimmed))
            {
                errors.Add($"Введите {fieldLabel.ToLowerInvariant()}");
            }
            else if (trimmed.Length < ValidationLimits.PersonNameMin)
            {
                errors.Add($"{fieldLabel} должно содержать минимум {ValidationLimits.PersonNameMin} символа");
            }
            else if (trimmed.Length > ValidationLimits.PersonNameMax)
            {
                errors.Add($"{fieldLabel} не должно превышать {ValidationLimits.PersonNameMax} символов");
            }

            return errors;
        }

        public static List<string> ValidateLogin(string? value)
        {
            var errors = new List<string>();
            var trimmed = value?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(trimmed))
            {
                errors.Add("Введите логин");
            }
            else if (trimmed.Length < ValidationLimits.LoginMin)
            {
                errors.Add($"Логин должен содержать минимум {ValidationLimits.LoginMin} символа");
            }
            else if (!IsValidLogin(trimmed))
            {
                errors.Add("Логин: только латиница, цифры, точка, дефис и подчёркивание");
            }

            return errors;
        }

        public static List<string> ValidatePassword(string? value, bool required)
        {
            var errors = new List<string>();

            if (required && string.IsNullOrWhiteSpace(value))
            {
                errors.Add("Введите пароль");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                return errors;
            }

            if (value.Length < ValidationLimits.PasswordMin)
            {
                errors.Add($"Пароль должен содержать минимум {ValidationLimits.PasswordMin} символов");
            }
            else if (value.Length > ValidationLimits.PasswordMax)
            {
                errors.Add($"Пароль не должен превышать {ValidationLimits.PasswordMax} символов");
            }

            return errors;
        }

        public static List<string> ValidatePasswordConfirmation(string? password, string? confirmPassword, bool required)
        {
            var errors = ValidatePassword(password, required);
            if (errors.Count > 0)
            {
                return errors;
            }

            if (required && password != confirmPassword)
            {
                errors.Add("Пароли не совпадают");
            }
            else if (!string.IsNullOrWhiteSpace(password) && password != confirmPassword)
            {
                errors.Add("Пароли не совпадают");
            }

            return errors;
        }

        private static string Truncate(string value, int maxLength) =>
            value.Length <= maxLength ? value : value[..maxLength];
    }
}
