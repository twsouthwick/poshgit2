﻿using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PoshGit2
{
    public sealed class RepositoryCache : IDisposable, IRepositoryCache
    {
        private readonly IDictionary<string, IRepositoryStatus> _repositories = new Dictionary<string, IRepositoryStatus>(StringComparer.OrdinalIgnoreCase);
        private readonly Func<string, ICurrentWorkingDirectory, IRepositoryStatus> _factory;
        private readonly ILogger _log;
        private readonly IFormatStatusString _writer;

        public RepositoryCache(ILogger log, Func<string, ICurrentWorkingDirectory, IRepositoryStatus> factory, IFormatStatusString writer)
        {
            _log = log;
            _factory = factory;
            _writer = writer;
        }

        public Task<IEnumerable<IRepositoryStatus>> GetAllReposAsync(CancellationToken cancellationToken)
        {
            var all = _repositories.Values
                .Select(o => new ReadonlyCopyRepositoryStatus(o))
                .ToList();

            return Task.FromResult<IEnumerable<IRepositoryStatus>>(all);
        }

        public Task<IRepositoryStatus> FindRepoAsync(ICurrentWorkingDirectory cwd, CancellationToken cancellationToken)
        {
            var @null = Task.FromResult<IRepositoryStatus>(null);

            if (!cwd.IsValid)
            {
                return @null;
            }

            var path = cwd.CWD;
            var repo = FindGitRepo(path);

            if (repo == null)
            {
                return @null;
            }

            IRepositoryStatus oldStatus;
            if (_repositories.TryGetValue(repo, out oldStatus))
            {
                _log.Information("Found repo: {Path}", repo);

                return Task.FromResult(new ReadonlyCopyRepositoryStatus(oldStatus, cwd) as IRepositoryStatus);
            }

            try
            {
                _log.Information("Creating repo: {Path}", repo);

                var status = _factory(repo, cwd);

                _repositories.Add(repo, status);

                return Task.FromResult(new ReadonlyCopyRepositoryStatus(status, cwd) as IRepositoryStatus);
            }
            catch (RepositoryNotFoundException)
            {
                return @null;
            }
        }

        public async Task<string> GetStatusStringAsync(string statusString, ICurrentWorkingDirectory cwd, CancellationToken token)
        {
            var status = await FindRepoAsync(cwd, token);

            return _writer.Format(statusString, status);
        }

        private static string FindGitRepo(string path)
        {
            if (path == null)
            {
                return null;
            }

            if (Directory.Exists(Path.Combine(path, ".git")))
            {
                return path;
            }
            else
            {
                var up = Path.GetDirectoryName(path);

                return string.Equals(up, path, StringComparison.OrdinalIgnoreCase) ? null : FindGitRepo(up);
            }
        }

        public Task<bool> RemoveRepoAsync(string path, CancellationToken cancellationToken)
        {
            var repoPath = FindGitRepo(path);

            return Task.FromResult(Remove(repoPath));
        }

        public Task<bool> ClearCacheAsync(CancellationToken cancellationToken)
        {
            _repositories.Clear();

            return Task.FromResult(true);
        }

        private bool Remove(string path)
        {
            var mainPath = FindGitRepo(path);

            if (mainPath == null)
            {
                return false;
            }

            IRepositoryStatus status;
            if (_repositories.TryGetValue(mainPath, out status))
            {
                (status as IDisposable)?.Dispose();
            }

            return _repositories.Remove(mainPath);
        }

        public void Dispose()
        {
            var all = _repositories.Keys.ToList();

            foreach (var repo in all)
            {
                Remove(repo);
            }
        }
    }
}
