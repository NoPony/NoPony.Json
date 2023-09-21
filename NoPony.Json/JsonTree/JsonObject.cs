using NoPony.Json.ObjectPool;
using System;
using System.Collections.Generic;

namespace NoPony.Json.JsonTree
{
    internal class JsonObject : IJsonObject
    {
        private readonly IPool<IJsonObject> _pool;

        private IJsonMemberList _members;
        private Dictionary<string, IJsonMember> _membersIndex = null;

        public IJsonMemberList Members { get => _members; set => _members = value; }

        internal JsonObject(IPool<IJsonObject> Pool)
        {
            _pool = Pool ?? throw new ArgumentNullException("Pool");
        }

        public IJsonValue this[string MemberName]
        {
            get
            {
                if (_membersIndex == null)
                {
                    _membersIndex = new Dictionary<string, IJsonMember>();

                    foreach (IJsonMember jm in Members)
                        _membersIndex[jm.Name.ToUpper()] = jm;
                }

                if (_membersIndex.TryGetValue(MemberName.ToUpper(), out IJsonMember result))
                    return result.Value;

                return null;
            }
        }

        // IPoolable
        public void Clear()
        {

        }

        public void Dispose()
        {
            if (Members != null)
            {
                Members.Dispose();
                Members = null;
            }

            if (_membersIndex != null)
            {
                _membersIndex.Clear();
                _membersIndex = null;
            }

                _pool.Release(this);
        }
    }
}
