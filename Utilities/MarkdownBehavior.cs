using System.Windows;
using System.Windows.Controls;

namespace LocalLLMChatVS.Utilities
{
    public static class MarkdownBehavior
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached(
                "Text",
                typeof(string),
                typeof(MarkdownBehavior),
                new PropertyMetadata(null, OnTextChanged));

        public static string GetText(DependencyObject obj) => (string)obj.GetValue(TextProperty);
        public static void SetText(DependencyObject obj, string value) => obj.SetValue(TextProperty, value);

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RichTextBox rtb)
                rtb.Document = MarkdownConverter.Convert(e.NewValue as string ?? string.Empty);
        }
    }
}
