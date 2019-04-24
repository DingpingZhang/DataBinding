using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace DataBinding.Tests
{
    public abstract class BindableBase : INotifyPropertyChanged
    {
        private readonly IDictionary<string, object> _propertyValueStorage = new ConcurrentDictionary<string, object>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual T GetProperty<T>(Expression<Func<T>> expression, T fallback = default, [CallerMemberName] string propertyName = null)
        {
            if (!_propertyValueStorage.ContainsKey(propertyName ?? throw new ArgumentNullException(nameof(propertyName))))
            {
                _propertyValueStorage.Add(propertyName, fallback);
                ExpressionObserver.Observes(expression,
                    (value, exception) =>
                    {
                        _propertyValueStorage[propertyName] = exception == null ? value : fallback;
                        OnPropertyChanged(propertyName);
                    });
            }

            return (T)_propertyValueStorage[propertyName];
        }

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
