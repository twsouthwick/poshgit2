namespace PoshGit2
{
    public interface IGitPromptSettings
    {
        PromptColor After { get; }
        PromptColor Before { get; }
        PromptColor BeforeIndex { get; }
        PromptColor Branch { get; }
        PromptColor BranchAhead { get; }
        PromptColor BranchBehind { get; }
        PromptColor BranchBehindAndAhead { get; }
        PromptColor Delim { get; }
        PromptColor Index { get; }
        PromptColor Untracked { get; }
        PromptColor Working { get; }
        string UntrackedText { get; }
        string AfterText { get; }
        string BeforeText { get; }
        string BeforeIndexText { get; }
        string DelimText { get; }
        bool ShowStatusWhenZero { get; }
        bool EnablePromptStatus { get; }
        bool EnableFileStatus { get; }
    }
}