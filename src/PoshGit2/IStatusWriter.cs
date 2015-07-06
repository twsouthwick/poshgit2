namespace PoshGit2
{
    public interface IStatusWriter
    {
        void WriteStatus(IRepositoryStatus status);
    }
}