using System;

namespace PoshGit2
{
    public sealed class FormatStatusString : IFormatStatusString
    {
        private static readonly IFormatProvider _formatProvider = new RepositoryStatusFormatProvider();

        public string Format(string formatString, IRepositoryStatus status)
        {
            var str = formatString.Replace("{", "{0:");

            return string.Format(_formatProvider, str, status);
        }

        private sealed class RepositoryStatusFormatProvider : IFormatProvider
        {
            private static readonly ICustomFormatter _customFormatter = new RepositoryStatusFormatter();

            public object GetFormat(Type formatType)
            {
                if (formatType == typeof(ICustomFormatter))
                {
                    return _customFormatter;
                }

                throw new InvalidOperationException();
            }

            private class RepositoryStatusFormatter : ICustomFormatter
            {
                public string Format(string format, object arg, IFormatProvider formatProvider)
                {
                    var path = format.Split('.');

                    return GetProperty(path, arg, 0).ToString();
                }

                private static object GetProperty(string[] path, object arg, int i)
                {
                    if (path.Length == i)
                    {
                        return arg;
                    }

                    var property = arg.GetType().GetProperty(path[i]);

                    if (property == null)
                    {
                        throw new ArgumentOutOfRangeException(string.Join(".", path), "Unknown value");
                    }

                    return GetProperty(path, property.GetValue(arg), i + 1);
                }
            }
        }
    }
}
