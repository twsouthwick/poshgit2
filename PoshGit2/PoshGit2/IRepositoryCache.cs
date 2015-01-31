using System.Collections;
using System.Collections.Generic;

namespace PoshGit2
{
    public interface IRepositoryCache 
    {
        IRepositoryStatus FindRepo(ICurrentWorkingDirectory cwd);
        IEnumerable<IRepositoryStatus> All { get; }
        void Remove(IRepositoryStatus repo);
    }
}