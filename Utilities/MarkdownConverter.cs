using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;

namespace LocalLLMChatVS.Utilities
{
    public static class MarkdownConverter
    {
        public static FlowDocument Convert(string markdown)
        {
            var doc = new FlowDocument { PagePadding = new Thickness(0) };

            if (string.IsNullOrEmpty(markdown))
                return doc;

            Color bg = ToMediaColor(VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey));
            Color fg = ToMediaColor(VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowTextColorKey));
            Color codeBg = IsLight(bg) ? Darken(bg, 0.06) : Lighten(bg, 0.10);

            var lines = markdown.Replace("\r\n", "\n").Split('\n');
            int i = 0;

            while (i < lines.Length)
            {
                string line = lines[i];

                // Fenced code block
                if (line.TrimStart().StartsWith("```"))
                {
                    var codeLines = new List<string>();
                    i++;
                    while (i < lines.Length && !lines[i].TrimStart().StartsWith("```"))
                    {
                        codeLines.Add(lines[i]);
                        i++;
                    }
                    i++; // consume closing ```

                    var codePara = new Paragraph
                    {
                        Background = new SolidColorBrush(codeBg),
                        Foreground = new SolidColorBrush(fg),
                        FontFamily = new FontFamily("Consolas, Courier New"),
                        FontSize = 12,
                        Padding = new Thickness(8),
                        Margin = new Thickness(0, 4, 0, 4),
                        LineHeight = 18
                    };
                    codePara.Inlines.Add(new Run(string.Join("\n", codeLines)));
                    doc.Blocks.Add(codePara);
                    continue;
                }

                // ATX headers
                var headerMatch = Regex.Match(line, @"^(#{1,6})\s+(.+)$");
                if (headerMatch.Success)
                {
                    int level = headerMatch.Groups[1].Value.Length;
                    double fontSize = level == 1 ? 20 : level == 2 ? 17 : level == 3 ? 15 : 13;
                    var para = new Paragraph { Margin = new Thickness(0, 6, 0, 2) };
                    var run = new Bold(new Run(headerMatch.Groups[2].Value))
                    {
                        FontSize = fontSize
                    };
                    para.Inlines.Add(run);
                    doc.Blocks.Add(para);
                    i++;
                    continue;
                }

                // Unordered list item
                var ulMatch = Regex.Match(line, @"^(\s*)[-*+]\s+(.+)$");
                if (ulMatch.Success)
                {
                    var list = new List { MarkerStyle = TextMarkerStyle.Disc, Margin = new Thickness(0, 2, 0, 2) };
                    while (i < lines.Length)
                    {
                        var m = Regex.Match(lines[i], @"^(\s*)[-*+]\s+(.+)$");
                        if (!m.Success) break;
                        var item = new ListItem(new Paragraph());
                        AddInlines(((Paragraph)item.Blocks.FirstBlock), m.Groups[2].Value, codeBg, fg);
                        list.ListItems.Add(item);
                        i++;
                    }
                    doc.Blocks.Add(list);
                    continue;
                }

                // Ordered list item
                var olMatch = Regex.Match(line, @"^(\s*)\d+\.\s+(.+)$");
                if (olMatch.Success)
                {
                    var list = new List { MarkerStyle = TextMarkerStyle.Decimal, Margin = new Thickness(0, 2, 0, 2) };
                    while (i < lines.Length)
                    {
                        var m = Regex.Match(lines[i], @"^(\s*)\d+\.\s+(.+)$");
                        if (!m.Success) break;
                        var item = new ListItem(new Paragraph());
                        AddInlines(((Paragraph)item.Blocks.FirstBlock), m.Groups[2].Value, codeBg, fg);
                        list.ListItems.Add(item);
                        i++;
                    }
                    doc.Blocks.Add(list);
                    continue;
                }

                // Horizontal rule
                if (Regex.IsMatch(line, @"^[-*_]{3,}\s*$"))
                {
                    var rule = new Paragraph(new Run("─────────────────────────────────"))
                    {
                        Foreground = new SolidColorBrush(fg) { Opacity = 0.4 },
                        Margin = new Thickness(0, 4, 0, 4)
                    };
                    doc.Blocks.Add(rule);
                    i++;
                    continue;
                }

                // Blank line — paragraph break
                if (string.IsNullOrWhiteSpace(line))
                {
                    i++;
                    continue;
                }

                // Regular paragraph: collect consecutive non-blank, non-special lines
                var paraLines = new List<string>();
                while (i < lines.Length
                    && !string.IsNullOrWhiteSpace(lines[i])
                    && !lines[i].TrimStart().StartsWith("```")
                    && !Regex.IsMatch(lines[i], @"^#{1,6}\s")
                    && !Regex.IsMatch(lines[i], @"^(\s*)[-*+]\s+")
                    && !Regex.IsMatch(lines[i], @"^(\s*)\d+\.\s+")
                    && !Regex.IsMatch(lines[i], @"^[-*_]{3,}\s*$"))
                {
                    paraLines.Add(lines[i]);
                    i++;
                }

                if (paraLines.Count > 0)
                {
                    var para = new Paragraph { Margin = new Thickness(0, 2, 0, 2) };
                    AddInlines(para, string.Join(" ", paraLines), codeBg, fg);
                    doc.Blocks.Add(para);
                }
            }

            return doc;
        }

        private static void AddInlines(Paragraph para, string text, Color codeBg, Color fg)
        {
            // Pattern order matters: code > bold+italic > bold > italic
            var pattern = new Regex(
                @"(`[^`]+`)" +              // inline code
                @"|(\*\*\*[^*]+\*\*\*)" +   // bold+italic ***
                @"|(\*\*[^*]+\*\*)" +       // bold **
                @"|(__[^_]+__)" +           // bold __
                @"|(\*[^*]+\*)" +           // italic *
                @"|(_[^_]+_)");             // italic _

            int pos = 0;
            foreach (Match m in pattern.Matches(text))
            {
                if (m.Index > pos)
                    para.Inlines.Add(new Run(text.Substring(pos, m.Index - pos)));

                string val = m.Value;
                if (val.StartsWith("`") && val.EndsWith("`"))
                {
                    para.Inlines.Add(new Run(val.Substring(1, val.Length - 2))
                    {
                        FontFamily = new FontFamily("Consolas, Courier New"),
                        FontSize = 12,
                        Background = new SolidColorBrush(codeBg),
                        Foreground = new SolidColorBrush(fg)
                    });
                }
                else if (val.StartsWith("***") && val.EndsWith("***"))
                {
                    var inner = val.Substring(3, val.Length - 6);
                    para.Inlines.Add(new Bold(new Italic(new Run(inner))));
                }
                else if ((val.StartsWith("**") && val.EndsWith("**")) ||
                         (val.StartsWith("__") && val.EndsWith("__")))
                {
                    var inner = val.Substring(2, val.Length - 4);
                    para.Inlines.Add(new Bold(new Run(inner)));
                }
                else if ((val.StartsWith("*") && val.EndsWith("*")) ||
                         (val.StartsWith("_") && val.EndsWith("_")))
                {
                    var inner = val.Substring(1, val.Length - 2);
                    para.Inlines.Add(new Italic(new Run(inner)));
                }

                pos = m.Index + m.Length;
            }

            if (pos < text.Length)
                para.Inlines.Add(new Run(text.Substring(pos)));
        }

        private static Color ToMediaColor(System.Drawing.Color c) => Color.FromArgb(c.A, c.R, c.G, c.B);

        private static bool IsLight(Color c) => (0.299 * c.R + 0.587 * c.G + 0.114 * c.B) / 255 > 0.5;

        private static Color Lighten(Color c, double amount) => Blend(c, Colors.White, amount);

        private static Color Darken(Color c, double amount) => Blend(c, Colors.Black, amount);

        private static Color Blend(Color c, Color target, double amount)
        {
            byte R(byte a, byte b) => (byte)(a + (b - a) * amount);
            return Color.FromRgb(R(c.R, target.R), R(c.G, target.G), R(c.B, target.B));
        }
    }
}
