using System;
using System.Collections.Generic;

namespace PoshGit2
{
    public abstract class ObservableBase<T> : IObservable<T>
    {
        private readonly ICollection<IObserver<T>> _observers = new List<IObserver<T>>();

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }

            return new CollectionUnsubscriber(_observers, observer);
        }

        protected void OnComplete()
        {
            foreach (var observer in _observers)
            {
                observer.OnCompleted();
            }
        }

        protected void Complete(Exception e)
        {
            foreach (var observer in _observers)
            {
                observer.OnError(e);
            }
        }

        protected void OnNext(T value)
        {
            foreach (var observer in _observers)
            {
                observer.OnNext(value);
            }
        }

        private sealed class CollectionUnsubscriber : IDisposable
        {
            private readonly ICollection<IObserver<T>> _observers;
            private readonly IObserver<T> _observer;
            public CollectionUnsubscriber(ICollection<IObserver<T>> observers, IObserver<T> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer))
                {
                    _observers.Remove(_observer);
                }
            }
        }
    }
}