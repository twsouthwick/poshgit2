namespace PoshGit2
{
    internal static class RepositoryStatusExtensions
    {
        internal static IRepositoryStatus Clone(this IRepositoryStatus status)
        {
            return new ReadonlyCopyRepositoryStatus(status);
        }

        private class ReadonlyCopyRepositoryStatus : IRepositoryStatus
        {
            public ReadonlyCopyRepositoryStatus(IRepositoryStatus status)
            {
                // TODO: Clone ChangeItemsCollection entries
                HasWorking = status.HasWorking;
                Branch = status.Branch;
                AheadBy = status.AheadBy;
                Working = status.Working;
                HasUntracked = status.HasUntracked;
                Index = status.Index;
                HasIndex = status.HasIndex;
                BehindBy = status.BehindBy;
                GitDir = status.GitDir;
            }

            public bool HasWorking { get; }
            public string Branch { get; }
            public int AheadBy { get; }
            public ChangedItemsCollection Working { get; }
            public bool HasUntracked { get; }
            public ChangedItemsCollection Index { get; }
            public bool HasIndex { get; }
            public int BehindBy { get; }
            public string GitDir { get; }
        }
    }
}
