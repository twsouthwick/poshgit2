using System;

namespace PoshGit2.Settings
{
    public class DefaultGitPromptSettings : IGitPromptSettings
    {
        public DefaultGitPromptSettings(ConsoleColor defaultBackgroundColor)
        {
            After = PromptColor.Create(ConsoleColor.Yellow, defaultBackgroundColor);
            Before = PromptColor.Create(ConsoleColor.Yellow, defaultBackgroundColor);
            BeforeIndex = PromptColor.Create(ConsoleColor.DarkGreen, defaultBackgroundColor);
            Branch = PromptColor.Create(ConsoleColor.Cyan, defaultBackgroundColor);
            BranchAhead = PromptColor.Create(ConsoleColor.Green, defaultBackgroundColor);
            BranchBehind = PromptColor.Create(ConsoleColor.Red, defaultBackgroundColor);
            BranchBehindAndAhead = PromptColor.Create(ConsoleColor.Yellow, defaultBackgroundColor);
            Delim = PromptColor.Create(ConsoleColor.Yellow, defaultBackgroundColor);
            Index = PromptColor.Create(ConsoleColor.DarkGreen, defaultBackgroundColor);
            Untracked = PromptColor.Create(ConsoleColor.DarkRed, defaultBackgroundColor);
            Working = PromptColor.Create(ConsoleColor.DarkRed, defaultBackgroundColor);
        }

        public PromptColor After { get; }

        public string AfterText { get; } = "]";

        public PromptColor Before { get; }

        public PromptColor BeforeIndex { get; }

        public string BeforeIndexText { get; } = "";

        public string BeforeText { get; } = " [";

        public PromptColor Branch { get; }

        public PromptColor BranchAhead { get; }

        public PromptColor BranchBehind { get; }

        public PromptColor BranchBehindAndAhead { get; }

        public PromptColor Delim { get; }

        public string DelimText { get; } = " |";

        public bool EnableFileStatus { get; } = true;

        public bool EnablePromptStatus { get; } = true;

        public PromptColor Index { get; }

        public bool ShowStatusWhenZero { get; } = true;

        public PromptColor Untracked { get; }

        public string UntrackedText { get; } = " !";

        public PromptColor Working { get; }
    }
}
