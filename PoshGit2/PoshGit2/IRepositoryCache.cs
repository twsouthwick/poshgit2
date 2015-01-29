namespace PoshGit2
{
    public interface IRepositoryCache
    {
        IRepositoryStatus FindRepo(string path);
        IRepositoryStatus GetCurrentRepo();
    }
}