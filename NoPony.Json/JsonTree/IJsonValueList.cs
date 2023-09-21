using NoPony.Json.ObjectPool;
using System.Collections.Generic;

namespace NoPony.Json.JsonTree
{
    public interface IJsonValueList : IList<IJsonValue>, IPoolable
    {
    }
}
