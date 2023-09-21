using NoPony.Json.ObjectPool;
using System;
using System.Collections.Generic;

namespace NoPony.Json.JsonTree
{
    internal class JsonMemberList : List<IJsonMember>, IJsonMemberList
    {
        private readonly IPool<IJsonMemberList> _pool;

        internal JsonMemberList(IPool<IJsonMemberList> Pool)
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
