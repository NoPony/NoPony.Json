using NoPony.Json.ObjectPool;

namespace NoPony.Json.JsonTree
{
    public interface IJsonValue : IPoolable
    {
        JsonValueType? Type { get; set; }
        IJsonObject ObjectValue { get; set; }
        IJsonArray ArrayValue { get; set; }
        string StringValue { get; set; }
        decimal? NumberValue { get; set; }
        JsonLiteral? LiteralValue { get; set; }
    }
}
