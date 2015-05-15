namespace PoshGit2
{
    public class RepoSearchCommands
    {
        public interface IRepoSearchCommand { }

        public class RemoveRepoCommand : IRepoSearchCommand
        {
            public string Path { get; }

            public RemoveRepoCommand(string path)
            {
                Path = path;
            }
        }

        public class GetRepoCommand : StringCurrentWorkingDirectory, IRepoSearchCommand
        {
            public GetRepoCommand(string cwd)
                : base(cwd)
            { }
        }

        public class QuitCommand : IRepoSearchCommand
        {
            public static IRepoSearchCommand Instance = new QuitCommand();
        }

        public class ShowAllCommand : IRepoSearchCommand
        {
            public static IRepoSearchCommand Instance = new ShowAllCommand();
        }
    }
}
