using System;

namespace NoPony.Json.ObjectPool
{
    public interface IPoolable : IDisposable
    {
        void Clear();
    }
}
