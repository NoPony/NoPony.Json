using System;

namespace NoPony.Json.ObjectPool
{
    internal interface IPool<T> where T: IPoolable
    {
        T Aquire();
        void Release(T PoolableObject);
    }
}
