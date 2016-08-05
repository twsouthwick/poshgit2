namespace PoshGit2
{
    public interface IFormatStatusString
    {
        string Format(string formatString, IRepositoryStatus status);
    }
}
