using System;

namespace PoshGit2
{
    public sealed class SerilogWrapper : ILogger
    {
        private readonly Serilog.ILogger _log;

        public SerilogWrapper(Serilog.ILogger log)
        {
            _log = log;
        }

        public void Debug(string messageTemplate, params object[] propertyValues)
        {
            _log.Debug(messageTemplate, propertyValues);
        }

        public void Debug(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _log.Debug(exception, messageTemplate, propertyValues);
        }

        public void Error(string messageTemplate, params object[] propertyValues)
        {
            _log.Error(messageTemplate, propertyValues);
        }

        public void Error(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _log.Error(exception, messageTemplate, propertyValues);
        }

        public void Fatal(string messageTemplate, params object[] propertyValues)
        {
            _log.Fatal(messageTemplate, propertyValues);
        }

        public void Fatal(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _log.Fatal(exception, messageTemplate, propertyValues);
        }

        public void Information(string messageTemplate, params object[] propertyValues)
        {
            _log.Information(messageTemplate, propertyValues);
        }

        public void Information(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _log.Information(exception, messageTemplate, propertyValues);
        }

        public void Verbose(string messageTemplate, params object[] propertyValues)
        {
            _log.Verbose(messageTemplate, propertyValues);
        }

        public void Verbose(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _log.Verbose(exception, messageTemplate, propertyValues);
        }

        public void Warning(string messageTemplate, params object[] propertyValues)
        {
            _log.Warning(messageTemplate, propertyValues);
        }

        public void Warning(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            _log.Warning(exception, messageTemplate, propertyValues);
        }
    }
}
