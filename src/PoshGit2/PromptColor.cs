using System;

namespace PoshGit2
{
    public struct PromptColor
    {
        public static PromptColor Create(ConsoleColor foreground, ConsoleColor background)
        {
            return new PromptColor
            {
                Foreground = foreground,
                Background = background
            };
        }

        public ConsoleColor Foreground { get; set; }

        public ConsoleColor Background { get; set; }
    }
}
