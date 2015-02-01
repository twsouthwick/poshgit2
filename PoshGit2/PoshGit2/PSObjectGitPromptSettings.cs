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

        public PSObjectGitPromptSettings(PSObject psObject)
        {
            _psobject = psObject;
        }

        public ConsoleColor AfterBackgroundColor
        {
            get
            {
                return ConvertToConsoleColor();
            }
        }

        public ConsoleColor AfterForegroundColor
        {
            get
            {
                return ConvertToConsoleColor();
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
                return ConvertToConsoleColor();
            }
        }

        public ConsoleColor BeforeForegroundColor
        {
            get
            {
                return ConvertToConsoleColor();
            }
        }

        public ConsoleColor BeforeIndexBackgroundColor
        {
            get
            {
                return ConvertToConsoleColor();
            }
        }

        public ConsoleColor BeforeIndexForegroundColor
        {
            get
            {
                return ConvertToConsoleColor();
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
                return ConvertToConsoleColor();
            }
        }

        public ConsoleColor BranchAheadForegroundColor
        {
            get
            {
                return ConvertToConsoleColor();
            }
        }

        public ConsoleColor BranchBackgroundColor
        {
            get
            {
                return ConvertToConsoleColor();
            }
        }

        public ConsoleColor BranchBehindAndAheadBackgroundColor
        {
            get
            {
                return ConvertToConsoleColor();
            }
        }

        public ConsoleColor BranchBehindAndAheadForegroundColor
        {
            get
            {
                return ConvertToConsoleColor();
            }
        }

        public ConsoleColor BranchBehindBackgroundColor
        {
            get
            {
                return ConvertToConsoleColor();
            }
        }

        public ConsoleColor BranchBehindForegroundColor
        {
            get
            {
                return ConvertToConsoleColor();
            }
        }

        public ConsoleColor BranchForegroundColor
        {
            get
            {
                return ConvertToConsoleColor();
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
                return ConvertToConsoleColor();
            }
        }

        public ConsoleColor DelimBackgroundColor
        {
            get
            {
                return ConvertToConsoleColor();
            }
        }

        public ConsoleColor DelimForegroundColor
        {
            get
            {
                return ConvertToConsoleColor();
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
                return ConvertToConsoleColor();
            }
        }

        public ConsoleColor IndexForegroundColor
        {
            get
            {
                return ConvertToConsoleColor();
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
                return ConvertToConsoleColor();
            }
        }

        public ConsoleColor UntrackedForegroundColor
        {
            get
            {
                return ConvertToConsoleColor();
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
                return ConvertToConsoleColor();
            }
        }

        public ConsoleColor WorkingForegroundColor
        {
            get
            {
                return ConvertToConsoleColor();
            }
        }

        private ConsoleColor ConvertToConsoleColor([CallerMemberName] string propertyName = null)
        {
            var property = _psobject.Properties[propertyName];

            if (property != null)
            {

                ConsoleColor output;
                if (Enum.TryParse(property.Value.ToString(), out output))
                {
                    return output;
                }
            }

            return ConsoleColor.White;
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