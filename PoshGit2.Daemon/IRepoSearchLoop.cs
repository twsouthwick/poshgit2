using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoshGit2
{
    public interface IRepoSearch
    {
        Task<RepoSearchCommands.IRepoSearchCommand> GetCommandAsync(CancellationToken token);
        Task SendResultAsync(IEnumerable<IRepositoryStatus> repos, CancellationToken token);
    }
}