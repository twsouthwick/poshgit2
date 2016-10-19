namespace PoshGit2
{
    public class DefaultStatusWriterProvider : IStatusWriterProvider
    {
        public IStatusWriter GetStatusWriter(IGitPromptSettings settings)
        {
            if (settings != null)
            {
                return new VT100StatusWriter(settings);
            }
            else
            {
                return new PlainTextStatusWriter();
            }
        }
    }
}
