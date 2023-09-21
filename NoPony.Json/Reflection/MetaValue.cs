namespace NoPony.Json.Reflection
{
    internal class MetaValue
    {
        internal MetaValueType ValueType;
        internal MetaObject Type { get; set; }

        public MetaObject ObjectValue { get; set; }
        public MetaContainer ContainerValue { get; set; }
        public string StringValue { get; set; }
        public decimal? NumberValue { get; set; }
        public bool? BooleanValue { get; set; }
    }
}
