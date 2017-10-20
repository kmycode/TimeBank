using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace TimeBank.Models.Common
{
    static class MyExtensions
    {
        public static IDisposable Bind<T>(this IObservable<T> from, ReactiveProperty<T> to)
        {
            return from.Subscribe(value => to.Value = value);
        }

        public static IDisposable Bind<T>(this ReactiveProperty<T> from, ReactiveProperty<T> to)
        {
            to.Value = from.Value;
            return Bind((IObservable<T>)from, to);
        }

        public static IDisposable BindBidirectional<T>(this ReactiveProperty<T> primary, ReactiveProperty<T> secondary)
        {
            primary.Value = secondary.Value;

            return new DisposableCollection
            {
                primary.Subscribe(value =>
                {
                    if (!secondary.Value.Equals(value))
                    {
                        secondary.Value = value;
                    }
                }),
                secondary.Subscribe(value =>
                {
                    if (!primary.Value.Equals(value))
                    {
                        primary.Value = value;
                    }
                }),
            };
        }

        private class DisposableCollection : IEnumerable<IDisposable>, IDisposable
        {
            private readonly List<IDisposable> items = new List<IDisposable>();

            public void Add(IDisposable item)
            {
                this.items.Add(item);
            }

            public void Dispose()
            {
                foreach (var item in this)
                {
                    item.Dispose();
                }
            }

            public IEnumerator<IDisposable> GetEnumerator() => this.items.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => this.items.GetEnumerator();
        }
    }
}
