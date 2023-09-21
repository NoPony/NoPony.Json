using NoPony.Json.ObjectPool;
using NoPony.Json.Parse;
using System;

namespace NoPony.Json.JsonTree
{
    internal class JsonResult : IJsonObject
    {
        private readonly IPool<JsonResult> _pool;

        private IParser _parser;
        internal IParser Parser
        {
            get => _parser;
            set => _parser = value;
        }

        public IJsonMemberList Members
        {
            get => _parser.Result.Members;
            set => throw new Exception("GTFOOH");
        }

        public IJsonValue this[string MemberName] => _parser.Result[MemberName];

        //public IParserPoolCollection Pools { get => _pools; set => _pools = value; }

        internal JsonResult(IPool<JsonResult> Pool)
        {
            _pool = Pool ?? throw new ArgumentNullException("Pool");
            _parser = null;
        }

        // IPoolable
        public void Clear()
        {

        }

        public void Dispose()
        {
            _parser?.Dispose();
            _parser = null;

            _pool.Release(this);
        }
    }
}
