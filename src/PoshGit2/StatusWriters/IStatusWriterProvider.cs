namespace PoshGit2
{
    public interface IStatusWriterProvider
    {
        IStatusWriter GetStatusWriter(IGitPromptSettings settings);
    }
}
