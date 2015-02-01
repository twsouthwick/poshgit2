using System;

namespace PoshGit2
{
    public class DelegateObserver : IObserver<FileChangedStatus>
    {
        private readonly Action<FileChangedStatus> _action;
        public DelegateObserver(Action<FileChangedStatus> action)
        {
            _action = action;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(FileChangedStatus value)
        {
            _action?.Invoke(value);
        }
    }
}