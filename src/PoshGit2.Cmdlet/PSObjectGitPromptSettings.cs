using System;
using System.Management.Automation;
using System.Runtime.CompilerServices;

namespace PoshGit2
{
    public class PSObjectGitPromptSettings : IGitPromptSettings
    {
        private readonly PSObject _psobject;
        private readonly IGitPromptSettings _defaultSettings;

        public PSObjectGitPromptSettings(PSObject psObject, IGitPromptSettings defaultSettings)
        {
            _psobject = psObject;
            _defaultSettings = defaultSettings;
        }

        public PromptColor After => GetPromptColor(_defaultSettings.After);

        public PromptColor Before => GetPromptColor(_defaultSettings.Before);

        public PromptColor BeforeIndex => GetPromptColor(_defaultSettings.BeforeIndex);

        public PromptColor Branch => GetPromptColor(_defaultSettings.Branch);

        public PromptColor BranchAhead => GetPromptColor(_defaultSettings.BranchAhead);

        public PromptColor BranchBehind => GetPromptColor(_defaultSettings.BranchBehind);

        public PromptColor BranchBehindAndAhead => GetPromptColor(_defaultSettings.BranchBehindAndAhead);

        public PromptColor Delim => GetPromptColor(_defaultSettings.Delim);

        public PromptColor Index => GetPromptColor(_defaultSettings.Index);

        public PromptColor Untracked => GetPromptColor(_defaultSettings.Untracked);

        public PromptColor Working => GetPromptColor(_defaultSettings.Working);

        public string UntrackedText => ConvertTo(_defaultSettings.UntrackedText);

        public bool EnablePromptStatus => ConvertTo(_defaultSettings.EnablePromptStatus);

        public bool EnableFileStatus => ConvertTo(_defaultSettings.EnableFileStatus);

        public string AfterText => ConvertTo(_defaultSettings.AfterText);

        public string BeforeText => ConvertTo(_defaultSettings.BeforeText);

        public string BeforeIndexText => ConvertTo(_defaultSettings.BeforeIndexText);

        public bool ShowStatusWhenZero => ConvertTo(_defaultSettings.ShowStatusWhenZero);

        public string DelimText => ConvertTo(_defaultSettings.DelimText);

        private PromptColor GetPromptColor(PromptColor defaultColors, [CallerMemberName] string propertyName = null)
        {
            var foreground = ConvertToConsoleColor($"{propertyName}ForegroundColor");
            var background = ConvertToConsoleColor($"{propertyName}BackgroundColor");

            return PromptColor.Create(foreground ?? defaultColors.Foreground, background ?? defaultColors.Background);
        }

        private ConsoleColor? ConvertToConsoleColor([CallerMemberName] string propertyName = null)
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

            return null;
        }

        private T ConvertTo<T>(T defaultValue, [CallerMemberName] string propertyName = null)
        {
            var property = _psobject.Properties[propertyName];

            if (property == null)
            {
                return defaultValue;
            }

            return (T)property.Value;
        }
    }
}