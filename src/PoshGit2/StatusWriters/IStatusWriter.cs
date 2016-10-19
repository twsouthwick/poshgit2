namespace PoshGit2
{
    public interface IStatusWriter
    {
        string Status { get; }

        void WriteStatus(IRepositoryStatus status);
    }
}