namespace PoshGit2.Status
{
    public interface IStatusWriter
    {
        void WriteStatus(IRepositoryStatus status);
    }
}