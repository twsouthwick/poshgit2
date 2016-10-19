using PoshGit2.Settings;
using System;
using System.Text;

namespace PoshGit2
{
    public class PlainTextStatusWriter : StatusWriter
    {
        private static readonly IGitPromptSettings s_settings = new DefaultGitPromptSettings(ConsoleColor.Black);

        private readonly StringBuilder _sb;

        public PlainTextStatusWriter()
            : base(s_settings)
        {
            _sb = new StringBuilder();
        }

        public override string Status => _sb.ToString();

        protected override void WriteColor(string msg, PromptColor color)
        {
            _sb.Append(msg);
        }
    }
}
