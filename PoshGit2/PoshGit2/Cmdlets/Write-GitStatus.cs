using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace PoshGit2.Cmdlets
{
    [Cmdlet(VerbsCommunications.Write, "GitStatus")]
    public class Write_GitStatus : DICmdlet
    {
        public IGitPromptSettings Settings { get; set; }
        public IRepositoryCache Cache { get; set; }
        public ICurrentWorkingDirectory CWD { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!Settings.EnablePromptStatus)
            {
                return;
            }

            var status = Cache.FindRepo(CWD);

            if(status == null)
            {
                return;
            }

            WriteColor(Settings.BeforeText, Settings.BeforeBackgroundColor, Settings.BeforeForegroundColor);

            WriteBranch(status);

            if (Settings.EnableFileStatus && status.HasIndex)
            {
                WriteColor(Settings.BeforeIndexText, Settings.BeforeIndexBackgroundColor, Settings.BeforeIndexForegroundColor);

                if (Settings.ShowStatusWhenZero || status.Index.Added.Any())
                {
                    WriteColor($" +{status.Index.Added.Count}", Settings.IndexBackgroundColor, Settings.IndexForegroundColor);
                }

                if (Settings.ShowStatusWhenZero || status.Index.Modified.Any())
                {
                    WriteColor($" ~{status.Index.Modified.Count}", Settings.IndexBackgroundColor, Settings.IndexForegroundColor);
                }

                if (Settings.ShowStatusWhenZero || status.Index.Deleted.Any())
                {
                    WriteColor($" -{status.Index.Deleted.Count}", Settings.IndexBackgroundColor, Settings.IndexForegroundColor);
                }

                if (status.Index.Unmerged.Any())
                {
                    WriteColor($" !{status.Index.Unmerged.Count}", Settings.IndexBackgroundColor, Settings.IndexForegroundColor);
                }

                if (status.HasWorking)
                {
                    WriteColor(Settings.DelimText, Settings.DelimBackgroundColor, Settings.DelimForegroundColor);
                }
            }

            if (Settings.EnableFileStatus && status.HasWorking)
            {
                if (Settings.ShowStatusWhenZero || status.Working.Added.Any())
                {
                    WriteColor($" +{status.Working.Added.Count}", Settings.WorkingBackgroundColor, Settings.WorkingForegroundColor);
                }

                if (Settings.ShowStatusWhenZero || status.Index.Modified.Any())
                {
                    WriteColor($" ~{status.Working.Modified.Count}", Settings.WorkingBackgroundColor, Settings.WorkingForegroundColor);
                }

                if (Settings.ShowStatusWhenZero || status.Index.Deleted.Any())
                {
                    WriteColor($" -{status.Working.Deleted.Count}", Settings.WorkingBackgroundColor, Settings.WorkingForegroundColor);
                }

                if (status.Index.Unmerged.Any())
                {
                    WriteColor($" !{status.Working.Unmerged.Count}", Settings.WorkingBackgroundColor, Settings.WorkingForegroundColor);
                }
            }

            if (status.HasUntracked)
            {
                WriteColor(Settings.UntrackedText, Settings.UntrackedBackgroundColor, Settings.UntrackedForegroundColor);
            }

            WriteColor(Settings.AfterText, Settings.AfterBackgroundColor, Settings.AfterForegroundColor);

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
                    BackgroundColor = Settings.BranchBehindAndAheadBackgroundColor,
                    ForegroundColor = Settings.BranchBehindAndAheadForegroundColor
                };
            }
            else if (status.BehindBy > 0)
            {
                return new StatusColor
                {
                    BackgroundColor = Settings.BranchBehindBackgroundColor,
                    ForegroundColor = Settings.BranchBehindForegroundColor
                };
            }
            else if (status.AheadBy > 0)
            {
                return new StatusColor
                {
                    BackgroundColor = Settings.BranchAheadBackgroundColor,
                    ForegroundColor = Settings.BranchAheadForegroundColor
                };
            }
            else
            {
                return new StatusColor
                {
                    BackgroundColor = Settings.BranchBackgroundColor,
                    ForegroundColor = Settings.BranchForegroundColor
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
