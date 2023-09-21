using System;

namespace NoPony.Json.Reflection
{
    internal class MetaContainer
    {
        internal Func<object> NewContainer { get; set; }
        internal Func<object> NewContainerItem { get; set; }
    }
}
