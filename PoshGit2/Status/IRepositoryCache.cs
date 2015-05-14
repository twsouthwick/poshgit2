using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoshGit2
{
    public interface IRepositoryCache
    {
        Task<IRepositoryStatus> FindRepo(ICurrentWorkingDirectory cwd, CancellationToken cancellationToken);
        Task<IEnumerable<IRepositoryStatus>> GetAllRepos(CancellationToken cancellationToken);
        Task RemoveRepo(IRepositoryStatus repo, CancellationToken cancellationToken);
    }
}