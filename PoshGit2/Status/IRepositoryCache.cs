﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PoshGit2
{
    public interface IRepositoryCache
    {
        Task<IRepositoryStatus> FindRepoAsync(ICurrentWorkingDirectory cwd, CancellationToken cancellationToken);
        Task<IEnumerable<IRepositoryStatus>> GetAllReposAsync(CancellationToken cancellationToken);
        Task RemoveRepoAsync(IRepositoryStatus repo, CancellationToken cancellationToken);
    }
}