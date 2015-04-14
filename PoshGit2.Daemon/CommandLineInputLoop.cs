using Autofac;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoshGit2
{
    public class CommandLineInputLoop : IRepoSearch
    {
        private readonly IRepositoryCache _cache;
        private readonly ILogger _log;
        private readonly ILifetimeScope _scope;

        public CommandLineInputLoop(ILogger log, IRepositoryCache cache, ILifetimeScope scope)
        {
            _log = log;
            _cache = cache;
            _scope = scope;
        }

        public Task<RepoSearchCommands.IRepoSearchCommand> GetCommandAsync(CancellationToken token)
        {
            return Task.FromResult(Run());
        }

        public Task SendResultAsync(IEnumerable<IRepositoryStatus> repos, CancellationToken token)
        {
            return Task.FromResult(true);
        }

        private RepoSearchCommands.IRepoSearchCommand Run()
        {
            Console.Write("> ");
            var line = Console.ReadLine();

            if (line == "#q")
            {
                return RepoSearchCommands.QuitCommand.Instance;
            }
            else if (line == "#all")
            {
                return RepoSearchCommands.ShowAllCommand.Instance;
            }
            else
            {
                return new RepoSearchCommands.GetRepoCommand(line);
            }
        }
    }
}