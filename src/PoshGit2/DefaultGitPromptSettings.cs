using System;
using System.Collections.Generic;
using System.Linq;

namespace PoshGit2
{
    public class DefaultGitPromptSettings : IGitPromptSettings
    {
        public ConsoleColor AfterBackgroundColor { get { return ConsoleColor.White; } }
        public ConsoleColor AfterForegroundColor { get { return ConsoleColor.White; } }
        public string AfterText { get { return string.Empty; } }
        public bool AutoRefreshIndex { get { return false; } }
        public ConsoleColor BeforeBackgroundColor { get { return ConsoleColor.White; } }
        public ConsoleColor BeforeForegroundColor { get { return ConsoleColor.White; } }
        public ConsoleColor BeforeIndexBackgroundColor { get { return ConsoleColor.White; } }
        public ConsoleColor BeforeIndexForegroundColor { get { return ConsoleColor.White; } }
        public string BeforeIndexText { get { return string.Empty; } }
        public string BeforeText { get { return string.Empty; } }
        public ConsoleColor BranchAheadBackgroundColor { get { return ConsoleColor.White; } }
        public ConsoleColor BranchAheadForegroundColor { get { return ConsoleColor.White; } }
        public ConsoleColor BranchBackgroundColor { get { return ConsoleColor.White; } }
        public ConsoleColor BranchBehindAndAheadBackgroundColor { get { return ConsoleColor.White; } }
        public ConsoleColor BranchBehindAndAheadForegroundColor { get { return ConsoleColor.White; } }
        public ConsoleColor BranchBehindBackgroundColor { get { return ConsoleColor.White; } }
        public ConsoleColor BranchBehindForegroundColor { get { return ConsoleColor.White; } }
        public ConsoleColor BranchForegroundColor { get { return ConsoleColor.White; } }
        public bool Debug { get { return false; } }
        public ConsoleColor DefaultForegroundColor { get { return ConsoleColor.White; } }
        public ConsoleColor DelimBackgroundColor { get { return ConsoleColor.White; } }
        public ConsoleColor DelimForegroundColor { get { return ConsoleColor.White; } }
        public string DelimText { get { return " |"; } }
        public string DescribeStyle { get { return string.Empty; } }
        public bool EnableFileStatus { get { return true; } }
        public bool EnablePromptStatus { get { return true; } }
        public bool EnableWindowTitle { get { return true; } }
        public ConsoleColor IndexBackgroundColor { get { return ConsoleColor.White; } }
        public ConsoleColor IndexForegroundColor { get { return ConsoleColor.White; } }
        public IEnumerable<string> RepositoriesInWhichToDisableFileStatus { get { return Enumerable.Empty<string>(); } }
        public bool ShowStatusWhenZero { get { return true; } }
        public ConsoleColor UntrackedBackgroundColor { get { return ConsoleColor.White; } }
        public ConsoleColor UntrackedForegroundColor { get { return ConsoleColor.White; } }
        public string UntrackedText { get { return " !"; } }
        public ConsoleColor WorkingBackgroundColor { get { return ConsoleColor.White; } }
        public ConsoleColor WorkingForegroundColor { get { return ConsoleColor.White; } }
    }
}
