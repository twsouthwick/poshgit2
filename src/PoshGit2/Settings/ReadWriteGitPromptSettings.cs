namespace PoshGit2.Settings
{
    public class ReadWriteGitPromptSettings : IGitPromptSettings
    {
        public PromptColor After { get; set; }

        public string AfterText { get; set; }

        public PromptColor Before { get; set; }

        public PromptColor BeforeIndex { get; set; }

        public string BeforeIndexText { get; set; }

        public string BeforeText { get; set; }

        public PromptColor Branch { get; set; }

        public PromptColor BranchAhead { get; set; }

        public PromptColor BranchBehind { get; set; }

        public PromptColor BranchBehindAndAhead { get; set; }

        public PromptColor Delim { get; set; }

        public string DelimText { get; set; }

        public bool EnableFileStatus { get; set; }

        public bool EnablePromptStatus { get; set; }

        public PromptColor Index { get; set; }

        public bool ShowStatusWhenZero { get; set; }

        public PromptColor Untracked { get; set; }

        public string UntrackedText { get; set; }

        public PromptColor Working { get; set; }
    }
}
