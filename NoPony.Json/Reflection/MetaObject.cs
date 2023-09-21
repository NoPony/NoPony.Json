using System;
using System.Collections.Generic;

namespace NoPony.Json.Reflection
{
    internal class MetaObject
    {
        internal string Name { get; set; }
        internal Dictionary<string, MetaMember> Members { get; set; }

        internal Func<object> New { get; set; }
    }
}
