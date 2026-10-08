using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ProjectManagementSystem.WPF.Helpers
{
    public enum InputFieldKind
    {
        None,
        PersonName,
        Login,
        Password,
        Search,
        Title,
        ShortDescription,
        LongDescription,
        Comment,
        PageNumber
    }

    public static class TextBoxInputRestriction
    {
        public static readonly DependencyProperty KindProperty =
            DependencyProperty.RegisterAttached(
                "Kind",
                typeof(InputFieldKind),
                typeof(TextBoxInputRestriction),
                new PropertyMetadata(InputFieldKind.None, OnKindChanged));

        public static InputFieldKind GetKind(DependencyObject obj) =>
            (InputFieldKind)obj.GetValue(KindProperty);

        public static void SetKind(DependencyObject obj, InputFieldKind value) =>
            obj.SetValue(KindProperty, value);

        private static readonly DependencyProperty HandlerProperty =
            DependencyProperty.RegisterAttached(
                "Handler",
                typeof(InputRestrictionHandler),
                typeof(TextBoxInputRestriction),
                new PropertyMetadata(null));

        private static void OnKindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBox textBox)
            {
                return;
            }

            if (GetHandler(textBox) is { } existing)
            {
                existing.Detach(textBox);
                SetHandler(textBox, null);
            }

            var kind = (InputFieldKind)e.NewValue;
            if (kind == InputFieldKind.None)
            {
                return;
            }

            var handler = new InputRestrictionHandler(kind);
            SetHandler(textBox, handler);
            handler.Attach(textBox);
        }

        private static InputRestrictionHandler? GetHandler(DependencyObject obj) =>
            (InputRestrictionHandler?)obj.GetValue(HandlerProperty);

        private static void SetHandler(DependencyObject obj, InputRestrictionHandler? value) =>
            obj.SetValue(HandlerProperty, value);

        private sealed class InputRestrictionHandler
        {
            private readonly InputFieldKind _kind;
            private TextBox? _textBox;

            public InputRestrictionHandler(InputFieldKind kind)
            {
                _kind = kind;
            }

            public void Attach(TextBox textBox)
            {
                _textBox = textBox;
                textBox.MaxLength = GetMaxLength(_kind);
                textBox.PreviewTextInput += OnPreviewTextInput;
                DataObject.AddPastingHandler(textBox, OnPaste);
            }

            public void Detach(TextBox textBox)
            {
                textBox.PreviewTextInput -= OnPreviewTextInput;
                DataObject.RemovePastingHandler(textBox, OnPaste);
                _textBox = null;
            }

            private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
            {
                e.Handled = !IsTextAllowed(e.Text);
            }

            private void OnPaste(object sender, DataObjectPastingEventArgs e)
            {
                if (_textBox == null || !e.DataObject.GetDataPresent(typeof(string)))
                {
                    return;
                }

                var pasted = e.DataObject.GetData(typeof(string)) as string ?? string.Empty;
                var sanitized = Sanitize(pasted);
                if (sanitized == pasted)
                {
                    return;
                }

                e.CancelCommand();
                InsertAtCaret(sanitized);
            }

            private void InsertAtCaret(string text)
            {
                if (_textBox == null || string.IsNullOrEmpty(text))
                {
                    return;
                }

                var selectionStart = _textBox.SelectionStart;
                var selectionLength = _textBox.SelectionLength;
                var current = _textBox.Text ?? string.Empty;
                var combined = current.Remove(selectionStart, selectionLength).Insert(selectionStart, text);
                var sanitized = Sanitize(combined);
                _textBox.Text = sanitized;
                _textBox.SelectionStart = Math.Min(selectionStart + text.Length, sanitized.Length);
            }

            private bool IsTextAllowed(string text)
            {
                foreach (var c in text)
                {
                    if (!IsCharAllowed(c))
                    {
                        return false;
                    }
                }

                return true;
            }

            private bool IsCharAllowed(char c) =>
                _kind switch
                {
                    InputFieldKind.PersonName => InputValidation.IsAllowedPersonNameChar(c),
                    InputFieldKind.Login => InputValidation.IsAllowedLoginChar(c),
                    InputFieldKind.Password => InputValidation.IsAllowedPasswordChar(c),
                    InputFieldKind.Search => InputValidation.IsAllowedSearchChar(c),
                    InputFieldKind.Title => InputValidation.IsAllowedTextChar(c, allowNewLine: false),
                    InputFieldKind.ShortDescription or InputFieldKind.LongDescription or InputFieldKind.Comment =>
                        InputValidation.IsAllowedTextChar(c, allowNewLine: true),
                    InputFieldKind.PageNumber => InputValidation.IsAllowedPageNumberChar(c),
                    _ => true
                };

            private string Sanitize(string value) =>
                _kind switch
                {
                    InputFieldKind.PersonName => InputValidation.SanitizePersonName(value),
                    InputFieldKind.Login => InputValidation.SanitizeLogin(value),
                    InputFieldKind.Password => InputValidation.SanitizePassword(value),
                    InputFieldKind.Search => InputValidation.SanitizeSearch(value),
                    InputFieldKind.Title => InputValidation.SanitizeTitle(value, ValidationLimits.TaskTitleMax),
                    InputFieldKind.ShortDescription => InputValidation.SanitizeMultilineText(value, ValidationLimits.ProjectDescriptionMax),
                    InputFieldKind.LongDescription => InputValidation.SanitizeMultilineText(value, ValidationLimits.TaskDescriptionMax),
                    InputFieldKind.Comment => InputValidation.SanitizeMultilineText(value, ValidationLimits.CommentMax),
                    InputFieldKind.PageNumber => InputValidation.SanitizePageNumber(value),
                    _ => value
                };

            private static int GetMaxLength(InputFieldKind kind) =>
                kind switch
                {
                    InputFieldKind.PersonName => ValidationLimits.PersonNameMax,
                    InputFieldKind.Login => ValidationLimits.LoginMax,
                    InputFieldKind.Password => ValidationLimits.PasswordMax,
                    InputFieldKind.Search => ValidationLimits.SearchMax,
                    InputFieldKind.Title => ValidationLimits.TaskTitleMax,
                    InputFieldKind.ShortDescription => ValidationLimits.ProjectDescriptionMax,
                    InputFieldKind.LongDescription => ValidationLimits.TaskDescriptionMax,
                    InputFieldKind.Comment => ValidationLimits.CommentMax,
                    InputFieldKind.PageNumber => 6,
                    _ => int.MaxValue
                };
        }
    }
}
