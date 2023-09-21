using NoPony.Json.ObjectPool;
using System;
using System.Collections.Generic;

namespace NoPony.Json.JsonTree
{
    internal class JsonValueList : List<IJsonValue>, IJsonValueList
    {
        private readonly IPool<IJsonValueList> _pool;

        internal JsonValueList(IPool<IJsonValueList> Pool)
        {
            _pool = Pool ?? throw new ArgumentNullException(nameof(Pool));
        }

        // IPoolable
        void IPoolable.Clear()
        {
            base.Clear();
        }

        public void Dispose()
        {
            _pool.Release(this);
        }
    }
}
