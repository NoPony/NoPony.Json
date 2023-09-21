using NoPony.Json.ObjectPool;
using System;

namespace NoPony.Json.JsonTree
{
    internal class JsonMember : IJsonMember
    {
        private readonly IPool<IJsonMember> _pool;

        private string _name;
        private IJsonValue _value;

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public IJsonValue Value
        {
            get => _value;
            set => _value = value;
        }

        internal JsonMember(IPool<IJsonMember> Pool)
        {
            _pool = Pool ?? throw new ArgumentNullException(nameof(Pool));
        }

        // IPoolable
        public void Clear()
        {
            _name = null;
            _value.Clear();
        }

        public void Dispose()
        {
            Value?.Dispose();

            _pool.Release(this);
        }
    }
}
