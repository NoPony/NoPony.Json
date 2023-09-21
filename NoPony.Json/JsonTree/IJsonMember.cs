using NoPony.Json.ObjectPool;

namespace NoPony.Json.JsonTree
{
    public interface IJsonMember : IPoolable
    {
        string Name { get; set; }
        IJsonValue Value { get; set; }
    }
}
