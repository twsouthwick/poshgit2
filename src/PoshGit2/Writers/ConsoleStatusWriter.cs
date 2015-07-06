using System;
using System.Linq;

namespace PoshGit2.Writers
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

            WriteColor(_settings.BeforeText, _settings.Before);

            WriteColor(status.Branch, GetBranchColor(status));

            if (_settings.EnableFileStatus && status.Index.HasAny)
            {
                WriteColor(_settings.BeforeIndexText, _settings.BeforeIndex);

                if (_settings.ShowStatusWhenZero || status.Index.Added.Any())
                {
                    WriteColor($" +{status.Index.Added.Count}", _settings.Index);
                }

                if (_settings.ShowStatusWhenZero || status.Index.Modified.Any())
                {
                    WriteColor($" ~{status.Index.Modified.Count}", _settings.Index);
                }

                if (_settings.ShowStatusWhenZero || status.Index.Deleted.Any())
                {
                    WriteColor($" -{status.Index.Deleted.Count}", _settings.Index);
                }

                if (status.Index.Unmerged.Any())
                {
                    WriteColor($" !{status.Index.Unmerged.Count}", _settings.Index);
                }

                if (status.Working.HasAny)
                {
                    WriteColor(_settings.DelimText, _settings.Delim);
                }
            }

            if (_settings.EnableFileStatus && status.Working.HasAny)
            {
                if (_settings.ShowStatusWhenZero || status.Working.Added.Any())
                {
                    WriteColor($" +{status.Working.Added.Count}", _settings.Working);
                }

                if (_settings.ShowStatusWhenZero || status.Index.Modified.Any())
                {
                    WriteColor($" ~{status.Working.Modified.Count}", _settings.Working);
                }

                if (_settings.ShowStatusWhenZero || status.Index.Deleted.Any())
                {
                    WriteColor($" -{status.Working.Deleted.Count}", _settings.Working);
                }

                if (status.Index.Unmerged.Any())
                {
                    WriteColor($" !{status.Working.Unmerged.Count}", _settings.Working);
                }
            }

            //if (status.HasUntracked)
            //{
            //    WriteColor(_settings.UntrackedText, _settings.Untracked.Background, _settings.Untracked.Foreground);
            //}

            WriteColor(_settings.AfterText, _settings.After);

            // TODO: Update Window title
        }


        private PromptColor GetBranchColor(IRepositoryStatus status)
        {
            if (status.BehindBy > 0 && status.AheadBy > 0)
            {
                return _settings.BranchBehindAndAhead;
            }
            else if (status.BehindBy > 0)
            {
                return _settings.BranchBehind;
            }
            else if (status.AheadBy > 0)
            {
                return _settings.BranchAhead;
            }
            else
            {
                return _settings.Branch;
            }
        }

        private void WriteColor(string msg, PromptColor color)
        {
            var previousForeground = Console.ForegroundColor;
            var previousBackground = Console.BackgroundColor;

            Console.ForegroundColor = color.Foreground;
            Console.BackgroundColor = color.Background;

            try
            {
                Console.Write(msg);
            }
            finally
            {
                Console.ForegroundColor = previousForeground;
                Console.BackgroundColor = previousBackground;
            }
        }
    }
}
