using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Runtime.CompilerServices;

namespace PoshGit2
{
    public class PSObjectGitPromptSettings : IGitPromptSettings
    {
        private readonly PSObject _psobject;

        public PSObjectGitPromptSettings(SessionState sessionState)
        {
            _psobject = sessionState.PSVariable.GetValue("GitPromptSettings") as PSObject;
        }

        public ConsoleColor AfterBackgroundColor
        {
            get
            {
                return ConvertTo<ConsoleColor>();
            }
        }

        public ConsoleColor AfterForegroundColor
        {
            get
            {
                return ConvertTo<ConsoleColor>();
            }
        }

        public string AfterText
        {
            get
            {
                return ConvertTo<string>();
            }
        }

        public bool AutoRefreshIndex
        {
            get
            {
                return ConvertTo<bool>();
            }
        }

        public ConsoleColor BeforeBackgroundColor
        {
            get
            {
                return ConvertTo<ConsoleColor>();
            }
        }

        public ConsoleColor BeforeForegroundColor
        {
            get
            {
                return ConvertTo<ConsoleColor>();
            }
        }

        public ConsoleColor BeforeIndexBackgroundColor
        {
            get
            {
                return ConvertTo<ConsoleColor>();
            }
        }

        public ConsoleColor BeforeIndexForegroundColor
        {
            get
            {
                return ConvertTo<ConsoleColor>();
            }
        }

        public string BeforeIndexText
        {
            get
            {
                return ConvertTo<string>();
            }
        }

        public string BeforeText
        {
            get
            {
                return ConvertTo<string>();
            }
        }

        public ConsoleColor BranchAheadBackgroundColor
        {
            get
            {
                return ConvertTo<ConsoleColor>();
            }
        }

        public ConsoleColor BranchAheadForegroundColor
        {
            get
            {
                return ConvertTo<ConsoleColor>();
            }
        }

        public ConsoleColor BranchBackgroundColor
        {
            get
            {
                return ConvertTo<ConsoleColor>();
            }
        }

        public ConsoleColor BranchBehindAndAheadBackgroundColor
        {
            get
            {
                return ConvertTo<ConsoleColor>();
            }
        }

        public ConsoleColor BranchBehindAndAheadForegroundColor
        {
            get
            {
                return ConvertTo<ConsoleColor>();
            }
        }

        public ConsoleColor BranchBehindBackgroundColor
        {
            get
            {
                return ConvertTo<ConsoleColor>();
            }
        }

        public ConsoleColor BranchBehindForegroundColor
        {
            get
            {
                return ConvertTo<ConsoleColor>();
            }
        }

        public ConsoleColor BranchForegroundColor
        {
            get
            {
                return ConvertTo<ConsoleColor>();
            }
        }

        public bool Debug
        {
            get
            {
                return ConvertTo<bool>();
            }
        }

        public ConsoleColor DefaultForegroundColor
        {
            get
            {
                return ConvertTo<ConsoleColor>();
            }
        }

        public ConsoleColor DelimBackgroundColor
        {
            get
            {
                return ConvertTo<ConsoleColor>();
            }
        }

        public ConsoleColor DelimForegroundColor
        {
            get
            {
                return ConvertTo<ConsoleColor>();
            }
        }

        public string DelimText
        {
            get
            {
                return ConvertTo<string>();
            }
        }

        public string DescribeStyle
        {
            get
            {
                return ConvertTo<string>();
            }
        }

        public bool EnableFileStatus
        {
            get
            {
                return ConvertTo<bool>();
            }
        }

        public bool EnablePromptStatus
        {
            get
            {
                return ConvertTo<bool>();
            }
        }

        public bool EnableWindowTitle
        {
            get
            {
                return ConvertTo<bool>();
            }
        }

        public ConsoleColor IndexBackgroundColor
        {
            get
            {
                return ConvertTo<ConsoleColor>();
            }
        }

        public ConsoleColor IndexForegroundColor
        {
            get
            {
                return ConvertTo<ConsoleColor>();
            }
        }

        public IEnumerable<string> RepositoriesInWhichToDisableFileStatus
        {
            get
            {
                // TODO: Throws InvalidCastException
                return ConvertTo<string[]>() ?? Enumerable.Empty<string>();
            }
        }


        public bool ShowStatusWhenZero
        {
            get
            {
                return ConvertTo<bool>();
            }
        }

        public ConsoleColor UntrackedBackgroundColor
        {
            get
            {
                return ConvertTo<ConsoleColor>();
            }
        }

        public ConsoleColor UntrackedForegroundColor
        {
            get
            {
                return ConvertTo<ConsoleColor>();
            }
        }

        public string UntrackedText
        {
            get
            {
                return ConvertTo<string>();
            }
        }

        public ConsoleColor WorkingBackgroundColor
        {
            get
            {
                return ConvertTo<ConsoleColor>();
            }
        }

        public ConsoleColor WorkingForegroundColor
        {
            get
            {
                return ConvertTo<ConsoleColor>();
            }
        }

        private T ConvertTo<T>([CallerMemberName] string propertyName = null)
        {
            var property = _psobject.Properties[propertyName];

            if (property == null)
            {
                return default(T);
            }

            return (T)property.Value;
        }
    }
}