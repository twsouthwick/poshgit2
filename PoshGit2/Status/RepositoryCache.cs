﻿using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PoshGit2
{
    public sealed class RepositoryCache : IDisposable, IRepositoryCache
    {
        private readonly IDictionary<string, IRepositoryStatus> _repositories = new Dictionary<string, IRepositoryStatus>(StringComparer.OrdinalIgnoreCase);
        private readonly Func<string, IRepositoryStatus> _factory;

        public RepositoryCache(Func<string, IRepositoryStatus> factory)
        {
            _factory = factory;
        }

        public IEnumerable<IRepositoryStatus> All
        {
            get
            {
                return _repositories.Values.Select(o => o.Clone()).ToList();
            }
        }

        public IRepositoryStatus FindRepo(ICurrentWorkingDirectory cwd)
        {
            if(!cwd.IsValid)
            {
                return null;
            }

            var path = cwd.CWD;
            var repo = FindGitRepo(path);

            if (repo == null)
            {
                return null;
            }

            IRepositoryStatus oldStatus;
            if (_repositories.TryGetValue(repo, out oldStatus))
            {
                Trace.WriteLine($"Found repo for {repo}");

                return oldStatus.Clone();
            }

            try
            {
                Trace.WriteLine($"Creating new repo for {repo}");

                var status = _factory(repo);

                _repositories.Add(repo, status);

                return status.Clone();
            }
            catch (RepositoryNotFoundException)
            {
                return null;
            }
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

                if (String.Equals(up, path, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
                else
                {
                    return FindGitRepo(up);
                }
            }
        }

        public void Remove(IRepositoryStatus repository)
        {
            var storedRepo = _repositories
                .Where(r => string.Equals(r.Value.GitDir, repository.GitDir, StringComparison.CurrentCultureIgnoreCase))
                .FirstOrDefault();

            Remove(storedRepo.Key);
        }

        private void Remove(string path)
        {
            var mainPath = FindGitRepo(path);

            if (mainPath == null)
            {
                return;
            }

            IRepositoryStatus status;
            if (_repositories.TryGetValue(mainPath, out status))
            {
                (status as IDisposable)?.Dispose();
            }

            _repositories.Remove(mainPath);
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