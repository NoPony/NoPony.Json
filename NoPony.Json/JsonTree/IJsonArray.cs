using NoPony.Json.ObjectPool;

namespace NoPony.Json.JsonTree
{
    public interface IJsonArray : IPoolable
    {
        IJsonValueList Values { get; set; }
        IJsonValue this[int Index] { get; }
    }
}
