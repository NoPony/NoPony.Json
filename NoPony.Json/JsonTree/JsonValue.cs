using NoPony.Json.ObjectPool;
using System;

namespace NoPony.Json.JsonTree
{
    internal class JsonValue : IJsonValue
    {
        private readonly IPool<IJsonValue> _pool;

        private JsonValueType? _type = null;
        private IJsonObject _objectValue = null;
        private IJsonArray _arrayValue = null;
        private string _stringValue = null;
        private decimal? _numberValue = null;
        private JsonLiteral? _literalValue = null;

        internal JsonValue(IPool<IJsonValue> Pool)
        {
            _pool = Pool ?? throw new ArgumentNullException("Pool");
        }

        public JsonValueType? Type
        {
            get => _type;
            set => _type = value;
        }

        public IJsonObject ObjectValue
        {
            get => _objectValue;
            set => _objectValue = value;
        }

        public IJsonArray ArrayValue
        {
            get => _arrayValue;
            set => _arrayValue = value;
        }

        public string StringValue
        {
            get => _stringValue;
            set => _stringValue = value;
        }

        public decimal? NumberValue
        {
            get => _numberValue;
            set => _numberValue = value;
        }

        public JsonLiteral? LiteralValue
        {
            get => _literalValue;
            set => _literalValue = value;
        }

        // IPoolable
        public void Clear()
        {
            _type = null;

            _objectValue?.Dispose();
            _objectValue = null;
            _arrayValue?.Dispose();
            _arrayValue = null;
            _stringValue = null;
            _numberValue = null;
            _literalValue = null;
        }

        public void Dispose()
        {
            _type = null;

            _objectValue?.Dispose();
            _objectValue = null;
            _arrayValue?.Dispose();
            _arrayValue = null;
            _stringValue = null;
            _numberValue = null;
            _literalValue = null;

            _pool.Release(this);
        }
    }
}
