using System;
using System.Collections.Generic;

namespace PoshGit2
{
    public interface IGitPromptSettings
    {
        ConsoleColor BranchAheadForegroundColor { get; }
        ConsoleColor AfterBackgroundColor { get; }
        ConsoleColor DelimForegroundColor { get; }
        string DescribeStyle { get; }
        ConsoleColor DelimBackgroundColor { get; }
        ConsoleColor UntrackedBackgroundColor { get; }
        ConsoleColor BeforeForegroundColor { get; }
        ConsoleColor BranchAheadBackgroundColor { get; }
        string UntrackedText { get; }
        ConsoleColor BranchBackgroundColor { get; }
        ConsoleColor BeforeIndexForegroundColor { get; }
        string EnablePromptStatus { get; }
        bool Debug { get; }
        bool EnableFileStatus { get; }
        ConsoleColor DefaultForegroundColor { get; }
        string AfterText { get; }
        ConsoleColor BranchBehindForegroundColor { get; }
        IEnumerable<string> RepositoriesInWhichToDisableFileStatus { get; }
        ConsoleColor BranchForegroundColor { get; }
        ConsoleColor BranchBehindAndAheadBackgroundColor { get; }
        ConsoleColor BranchBehindAndAheadForegroundColor { get; }
        string BeforeText { get; }
        ConsoleColor WorkingBackgroundColor { get; }
        ConsoleColor WorkingForegroundColor { get; }
        string EnableWindowTitle { get; }
        ConsoleColor BranchBehindBackgroundColor { get; }
        string BeforeIndexText { get; }
        bool ShowStatusWhenZero { get; }
        string DelimText { get; }
        ConsoleColor BeforeBackgroundColor { get; }
        ConsoleColor UntrackedForegroundColor { get; }
        ConsoleColor IndexForegroundColor { get; }
        bool AutoRefreshIndex { get; }
        ConsoleColor IndexBackgroundColor { get; }
        ConsoleColor AfterForegroundColor { get; }
        ConsoleColor BeforeIndexBackgroundColor { get; }
    }
}