using NoPony.Json.ObjectPool;
using System;
using System.Text;

namespace NoPony.Json
{
    public class PooledStringBuilder : IPoolable
    {
        private readonly IPool<PooledStringBuilder> _pool;
        private readonly StringBuilder _impl;

        internal PooledStringBuilder(IPool<PooledStringBuilder> Pool)
        {
            _pool = Pool;
            _impl = new StringBuilder();
        }

        // implement the members of StringBuilder that we need
        // really should have implemented an interface

        public void Append(string value)
        {
            _impl.Append(value);
        }

        public void Append(char value)
        {
            _impl.Append(value);
        }

        public void Remove(int StartIndex, int Length)
        {
            _impl.Remove(StartIndex, Length);
        }

        public int Length
        {
            get
            {
                return _impl.Length;
            }
        }

        public override string ToString()
        {
            return _impl.ToString();
        }

        // IPoolable
        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _pool.Release(this);
        }
    }
}
