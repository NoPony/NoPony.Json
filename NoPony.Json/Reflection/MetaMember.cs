using System;

namespace NoPony.Json.Reflection
{
    internal class MetaMember
    {
        internal string Name { get; set; }
        internal MetaValue Value { get; set; }

        internal Func<object, object> Get { get; set; }
        internal Action<object, object> Set { get; set; }
    }
}
