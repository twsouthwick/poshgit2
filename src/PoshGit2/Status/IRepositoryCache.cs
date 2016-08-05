using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoshGit2
{
    public interface IRepositoryCache
    {
        Task<IRepositoryStatus> FindRepoAsync(ICurrentWorkingDirectory cwd, CancellationToken cancellationToken);
        Task<string> GetStatusStringAsync(string statusString, ICurrentWorkingDirectory cwd, CancellationToken cancellationToken);
        Task<IEnumerable<IRepositoryStatus>> GetAllReposAsync(CancellationToken cancellationToken);
        Task<bool> RemoveRepoAsync(string path, CancellationToken cancellationToken);
        Task<bool> ClearCacheAsync(CancellationToken token);
    }
}