using NoPony.Json.ObjectPool;
using System.Collections.Generic;

namespace NoPony.Json.JsonTree
{
    public interface IJsonMemberList : IList<IJsonMember>, IPoolable
    {
    }
}
