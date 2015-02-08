using System;
using System.Linq;

namespace PoshGit2.Status
{
    public class ConsoleStatusWriter : IStatusWriter
    {
        private readonly IGitPromptSettings _settings;

        public ConsoleStatusWriter(IGitPromptSettings settings)
        {
            _settings = settings;
        }

        public void WriteStatus(IRepositoryStatus status)
        {
            if (!_settings.EnablePromptStatus)
            {
                return;
            }

            WriteColor(_settings.BeforeText, _settings.BeforeBackgroundColor, _settings.BeforeForegroundColor);

            WriteBranch(status);

            if (_settings.EnableFileStatus && status.HasIndex)
            {
                WriteColor(_settings.BeforeIndexText, _settings.BeforeIndexBackgroundColor, _settings.BeforeIndexForegroundColor);

                if (_settings.ShowStatusWhenZero || status.Index.Added.Any())
                {
                    WriteColor($" +{status.Index.Added.Count}", _settings.IndexBackgroundColor, _settings.IndexForegroundColor);
                }

                if (_settings.ShowStatusWhenZero || status.Index.Modified.Any())
                {
                    WriteColor($" ~{status.Index.Modified.Count}", _settings.IndexBackgroundColor, _settings.IndexForegroundColor);
                }

                if (_settings.ShowStatusWhenZero || status.Index.Deleted.Any())
                {
                    WriteColor($" -{status.Index.Deleted.Count}", _settings.IndexBackgroundColor, _settings.IndexForegroundColor);
                }

                if (status.Index.Unmerged.Any())
                {
                    WriteColor($" !{status.Index.Unmerged.Count}", _settings.IndexBackgroundColor, _settings.IndexForegroundColor);
                }

                if (status.HasWorking)
                {
                    WriteColor(_settings.DelimText, _settings.DelimBackgroundColor, _settings.DelimForegroundColor);
                }
            }

            if (_settings.EnableFileStatus && status.HasWorking)
            {
                if (_settings.ShowStatusWhenZero || status.Working.Added.Any())
                {
                    WriteColor($" +{status.Working.Added.Count}", _settings.WorkingBackgroundColor, _settings.WorkingForegroundColor);
                }

                if (_settings.ShowStatusWhenZero || status.Index.Modified.Any())
                {
                    WriteColor($" ~{status.Working.Modified.Count}", _settings.WorkingBackgroundColor, _settings.WorkingForegroundColor);
                }

                if (_settings.ShowStatusWhenZero || status.Index.Deleted.Any())
                {
                    WriteColor($" -{status.Working.Deleted.Count}", _settings.WorkingBackgroundColor, _settings.WorkingForegroundColor);
                }

                if (status.Index.Unmerged.Any())
                {
                    WriteColor($" !{status.Working.Unmerged.Count}", _settings.WorkingBackgroundColor, _settings.WorkingForegroundColor);
                }
            }

            if (status.HasUntracked)
            {
                WriteColor(_settings.UntrackedText, _settings.UntrackedBackgroundColor, _settings.UntrackedForegroundColor);
            }

            WriteColor(_settings.AfterText, _settings.AfterBackgroundColor, _settings.AfterForegroundColor);

            // TODO: Update Window title
        }


        private class StatusColor
        {
            public ConsoleColor BackgroundColor { get; set; }
            public ConsoleColor ForegroundColor { get; set; }
        }

        private StatusColor GetBranchColor(IRepositoryStatus status)
        {
            if (status.BehindBy > 0 && status.AheadBy > 0)
            {
                return new StatusColor
                {
                    BackgroundColor = _settings.BranchBehindAndAheadBackgroundColor,
                    ForegroundColor = _settings.BranchBehindAndAheadForegroundColor
                };
            }
            else if (status.BehindBy > 0)
            {
                return new StatusColor
                {
                    BackgroundColor = _settings.BranchBehindBackgroundColor,
                    ForegroundColor = _settings.BranchBehindForegroundColor
                };
            }
            else if (status.AheadBy > 0)
            {
                return new StatusColor
                {
                    BackgroundColor = _settings.BranchAheadBackgroundColor,
                    ForegroundColor = _settings.BranchAheadForegroundColor
                };
            }
            else
            {
                return new StatusColor
                {
                    BackgroundColor = _settings.BranchBackgroundColor,
                    ForegroundColor = _settings.BranchForegroundColor
                };
            }
        }

        private void WriteBranch(IRepositoryStatus status)
        {
            var color = GetBranchColor(status);

            WriteColor(status.Branch, color.BackgroundColor, color.ForegroundColor);
        }

        private void WriteColor(string msg, ConsoleColor backgroundColor, ConsoleColor foregroundColor)
        {
            var previousForegroundColor = Console.ForegroundColor;
            var previousBackgroundColor = Console.BackgroundColor;

            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;

            try
            {
                Console.Write(msg);
            }
            finally
            {
                Console.ForegroundColor = previousForegroundColor;
                Console.BackgroundColor = previousBackgroundColor;
            }
        }
    }
}
