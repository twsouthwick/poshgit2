namespace PoshGit2
{
    public class Option<T>
        where T : class
    {
        public Option(T value)
        {
            Value = value;
        }

        public T Value { get; }

        public bool HasValue { get { return Value != null; } }
    }
}