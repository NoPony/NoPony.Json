using NoPony.Json.ObjectPool;
using System;

namespace NoPony.Json.JsonTree
{
    internal class JsonArray : IJsonArray
    {
        private readonly IPool<IJsonArray> _pool;

        private IJsonValueList _values;
        public IJsonValueList Values
        {
            get => _values;
            set => _values = value;
        }

        internal JsonArray(IPool<IJsonArray> Pool)
        {
            _pool = Pool ?? throw new ArgumentNullException("Pool");
        }

        public IJsonValue this[int Index]
        {
            get
            {
                return Values[Index];
            }
        }

        // IPoolable
        void IPoolable.Clear()
        {
            _values.Clear();
        }

        public void Dispose()
        {
            _pool?.Release(this);
        }
    }
}
