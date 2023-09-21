using NoPony.Json.ObjectPool;

namespace NoPony.Json.JsonTree
{
    public interface IJsonObject : IPoolable
    {
        IJsonMemberList Members { get; set; }
        IJsonValue this[string MemberName] { get; }
    }
}
