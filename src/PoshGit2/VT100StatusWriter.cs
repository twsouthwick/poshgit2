using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PoshGit2
{
    public class VT100StatusWriter : StatusWriter
    {
        private static readonly Dictionary<ConsoleColor, int> s_map = new Dictionary<ConsoleColor, int>
        {
            { ConsoleColor.Black, 0 },
            { ConsoleColor.Red, 1 },
            { ConsoleColor.DarkRed, 1 },
            { ConsoleColor.Green, 2 },
            { ConsoleColor.DarkGreen, 2 },
            { ConsoleColor.Yellow, 3 },
            { ConsoleColor.DarkYellow, 3 },
            { ConsoleColor.Blue, 4 },
            { ConsoleColor.DarkBlue, 4 },
            { ConsoleColor.Magenta, 5 },
            { ConsoleColor.DarkMagenta, 5 },
            { ConsoleColor.Cyan, 6 },
            { ConsoleColor.DarkCyan, 6 },
            { ConsoleColor.White, 7 }
        };

        private static readonly HashSet<ConsoleColor> s_darkColors = new HashSet<ConsoleColor>
        {
            ConsoleColor.DarkBlue,
            ConsoleColor.DarkCyan,
            ConsoleColor.DarkGray,
            ConsoleColor.DarkGreen,
            ConsoleColor.DarkMagenta,
            ConsoleColor.DarkRed,
            ConsoleColor.DarkYellow
        };

        private readonly StringBuilder _sb;

        public VT100StatusWriter(IGitPromptSettings settings)
            : base(settings)
        {
            _sb = new StringBuilder();
        }

        public string Status => _sb.ToString();

        private const char ESC = (char)0x1B;

        private void WriteColor(ConsoleColor color, bool background)
        {
            if (s_darkColors.Contains(color))
            {
                _sb.Append("2;");
            }

            _sb.Append(background ? '4' : '3');

            int m;
            if (s_map.TryGetValue(color, out m))
            {
                _sb.Append(m);
            }
            else
            {
                Debug.Fail($"Unknown color: {color}");
            }
        }

        private void ResetColor()
        {
            _sb.Append(ESC);
            _sb.Append("[0m");
        }

        protected override void WriteColor(string msg, PromptColor color)
        {
            _sb.Append(ESC);
            _sb.Append('[');

            WriteColor(color.Background, true);
            _sb.Append(';');
            WriteColor(color.Foreground, false);

            _sb.Append('m');

            _sb.Append(msg);

            ResetColor();
        }

    }
}
