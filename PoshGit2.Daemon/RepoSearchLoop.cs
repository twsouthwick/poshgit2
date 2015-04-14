using Autofac;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoshGit2
{
    public class RepoSearchLoop
    {
        private readonly IRepositoryCache _cache;
        private readonly ILogger _log;
        private readonly IRepoSearch _repoSearch;
        private readonly ILifetimeScope _scope;

        public RepoSearchLoop(IRepoSearch repoSearch, ILogger log, IRepositoryCache cache, ILifetimeScope scope)
        {
            _repoSearch = repoSearch;
            _log = log;
            _cache = cache;
            _scope = scope;
        }

        public async Task RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var command = await _repoSearch.GetCommandAsync(token);

                if (command is RepoSearchCommands.QuitCommand)
                {
                    return;
                }

                if (await ProcessGetAllCommandsAsync(command as RepoSearchCommands.ShowAllCommand, token))
                {
                    continue;
                }

                if (await ProcessGetRepoCommandAsync(command as RepoSearchCommands.GetRepoCommand, token))
                {
                    continue;
                }

                _log.Warning("Unknown command: {Command}", command);
            }
        }

        private async Task<bool> ProcessGetAllCommandsAsync(RepoSearchCommands.ShowAllCommand showAllCommand, CancellationToken token)
        {
            if (showAllCommand == null)
            {
                return false;
            }

            await _repoSearch.SendResultAsync(_cache.All, token);

            return true;

        }

        private async Task<bool> ProcessGetRepoCommandAsync(RepoSearchCommands.GetRepoCommand command, CancellationToken token)
        {
            if (command == null)
            {
                return false;
            }

            using (var scope = _scope.BeginLifetimeScope(builder => builder.RegisterInstance(command).As<ICurrentWorkingDirectory>()))
            {
                var repo = scope.Resolve<Option<IRepositoryStatus>>();

                if (repo.HasValue)
                {
                    _log.Information("{@Repo}", repo.Value);

                    await _repoSearch.SendResultAsync(new[] { repo.Value }, token);
                }
                else
                {
                    _log.Information("No repo found: {@Input}", command);

                    await _repoSearch.SendResultAsync(Enumerable.Empty<IRepositoryStatus>(), token);
                }
            }

            return true;
        }
    }
}
