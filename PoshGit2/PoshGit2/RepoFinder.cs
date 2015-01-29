using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PoshGit2
{
    public sealed class RepoFinder : IDisposable, IRepositoryCache
    {
        private readonly IDictionary<string, IRepositoryStatus> _repositories = new Dictionary<string, IRepositoryStatus>(StringComparer.OrdinalIgnoreCase);
        private readonly ICurrentWorkingDirectory _path;
        private readonly Func<string, IRepositoryStatus> _factory;

        public RepoFinder(ICurrentWorkingDirectory path, Func<string, IRepositoryStatus> factory)
        {
            _path = path;
            _factory = factory;
        }

        public IRepositoryStatus GetCurrentRepo()
        {
            return FindRepo(_path.CWD);
        }

        public IRepositoryStatus FindRepo(string path)
        {
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

        private void Remove(string path)
        {
            IRepositoryStatus status;

            if (_repositories.TryGetValue(path, out status))
            {
                (status as IDisposable)?.Dispose();
            }

            _repositories.Remove(path);
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
