using System.Management.Automation;

namespace PoshGit2
{
    [Cmdlet(VerbsData.Expand, "GitCommand")]
    public class ExpandGitCommand : AutofacCmdlet
    {
        [Parameter(Position = 0)]
        public string FullLine { get; set; }

        [Parameter(Position = 1)]
        public string LastWord { get; set; }

        public ITabCompleter Completer { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteObject(Completer.CompleteAsync(FullLine).Result);
        }
    }
}
