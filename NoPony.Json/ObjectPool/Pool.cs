using System;
using System.Collections.Concurrent;

namespace NoPony.Json.ObjectPool
{
    internal class Pool<T> : IPool<T> where T : IPoolable
    {
        private readonly ConcurrentBag<T> _pool;
        private readonly int _maxSize;
        private readonly Func<Pool<T>, T> _factory;

        private Pool()
        { }

        public Pool(Func<IPool<T>, T> Factory) : this(512, Factory)
        { }

        public Pool(int MaxSize, Func<IPool<T>, T> Factory)
        {
            _pool = new ConcurrentBag<T>();
            _maxSize = MaxSize;

            _factory = Factory ?? throw new ArgumentNullException(nameof(Factory));
        }

        public T Aquire()
        {
            T result;

            if (_pool.TryTake(out result))
                result.Clear();

            else
                result = _factory(this);

            return result;
        }

        public void Release(T PoolableObject)
        {
            if (_pool.Count < _maxSize)
                _pool.Add(PoolableObject);
        }
    }
}
