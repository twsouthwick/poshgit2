using System;

namespace PoshGit2
{
    public class ConsoleStatusWriter : StatusWriter
    {
        public ConsoleStatusWriter(IGitPromptSettings settings)
            : base(settings)
        {
        }

        protected override void WriteColor(string msg, PromptColor color)
        {
            var previousForeground = Console.ForegroundColor;
            var previousBackground = Console.BackgroundColor;

            Console.ForegroundColor = color.Foreground;
            Console.BackgroundColor = color.Background;

            try
            {
                Console.Write(msg);
            }
            finally
            {
                Console.ForegroundColor = previousForeground;
                Console.BackgroundColor = previousBackground;
            }
        }
    }
}
