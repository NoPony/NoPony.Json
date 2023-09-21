using NoPony.Json.JsonTree;
using NoPony.Json.ObjectPool;
using NoPony.Json.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace NoPony.Json.Translation
{
    internal class Encoder : IPoolable
    {
        private IPool<Encoder> _pool;
        private IPool<PooledStringBuilder> _stringBuilderPool;

        private Encoder()
        { }

        internal Encoder(IPool<Encoder> Pool)
        {
            _pool = Pool ?? throw new ArgumentNullException("Pool");
            _stringBuilderPool = new Pool<PooledStringBuilder>(32, pool => new PooledStringBuilder(pool));
        }

        internal string Encode(object instance, MetaObject metaObject)
        {
            return encodeObject(instance, metaObject);
        }

        private string encodeObject(object instance, MetaObject metaObject)
        {
            if (instance == null)
                return "null";

            using (PooledStringBuilder sb = _stringBuilderPool.Aquire())
            {
                sb.Append('{');

                foreach (KeyValuePair<string, MetaMember> tm in metaObject.Members)
                {
                    MetaMember metaMember = tm.Value;

                    sb.Append('\"');
                    sb.Append(metaMember.Name);
                    sb.Append('\"');
                    sb.Append(':');

                    sb.Append(encodeMember(metaMember.Get(instance), metaMember));

                    sb.Append(',');
                }

                sb.Remove(sb.Length - 1, 1);
                sb.Append('}');

                return sb.ToString();
            }
        }

        private string encodeMember(object instance, MetaMember metaMember)
        {
            switch (metaMember.Value.ValueType)
            {
                case MetaValueType.Object: return encodeMemberObject(instance, metaMember);
                case MetaValueType.Array: return encodeMemberArray(instance, metaMember);
                case MetaValueType.Number: return encodeMemberNumber(instance, metaMember);
                case MetaValueType.String: return encodeMemberString(instance, metaMember);
                case MetaValueType.Boolean: return encodeMemberLiteral(instance, metaMember);
                default: throw new ArgumentException("", nameof(metaMember));
            }
        }


        private string encodeMemberObject(object instance, MetaMember metaMember)
        {
            using (PooledStringBuilder sb = _stringBuilderPool.Aquire())
            {
                sb.Append('\"');
                sb.Append(metaMember.Name);
                sb.Append('\"');
                sb.Append(':');
                sb.Append(encodeObject(metaMember.Get(instance), metaMember.Value.Type));

                return sb.ToString();
            }
        }

        private string encodeMemberArray(object instance, MetaMember metaMember)
        {
            using (PooledStringBuilder sb = _stringBuilderPool.Aquire())
            {
                var collection = instance as IEnumerable;

                sb.Append('[');

                foreach (var item in collection)
                {
                    sb.Append(encodeObject(item, metaMember.Value.Type));
                    sb.Append(',');
                }

                sb.Remove(sb.Length - 1, 1);
                sb.Append(']');

                return sb.ToString();
            }
        }

        private string encodeMemberNumber(object instance, MetaMember metaMember)
        {
            if (instance is null)
                return "null";

            return $"{instance}";
        }

        private string encodeMemberString(object instance, MetaMember metaMember)
        {
            if (instance is null)
                return "null";

            return $"\"{(string)instance}\"";
        }

        private string encodeMemberLiteral(object instance, MetaMember metaMember)
        {
            if (instance is null)
                return "null";

            if ((bool)instance)
                return "true";

            return "false";
        }

        //private string encodeMemberValue(object instance, MetaMember metaMember)
        //{
        //    if (instance == null)
        //        return "null";

        //    switch (metaMember.Name)
        //    {
        //        case "System.Boolean":
        //            return (bool)instance ? "true" : "false";

        //        case "System.Char":
        //        case "System.String":
        //            return $"\"{(string)instance}\"";

        //        case "System.Byte":
        //        case "System.SByte":
        //        case "System.Decimal":
        //        case "System.Double":
        //        case "System.Single":
        //        case "System.Int32":
        //        case "System.UInt32":
        //        case "System.Int64":
        //        case "System.UInt64":
        //        case "System.Int16":
        //        case "System.UInt16":
        //            return $"{instance}";

        //        case "System.Object":
        //            return $"\"{instance}\"";

        //        default:
        //            throw new ArgumentOutOfRangeException("Invalid metaObject.QualifiedName in encodeValue()");
        //    }
        //}

        private string encodeMemberCollection(object instance, MetaMember metaMember)
        {
            using (PooledStringBuilder sb = _stringBuilderPool.Aquire())
            {

                sb.Append('[');

                foreach (var item in instance as IEnumerable)
                {
                    sb.Append(encodeObject(item, metaMember.Value.Type));
                    sb.Append(',');
                }

                sb.Remove(sb.Length - 1, 1);
                sb.Append(']');

                return sb.ToString();
            }
        }


        // IPoolable
        public void Clear()
        {

        }

        public void Dispose()
        {
            _pool.Release(this);
        }
    }
}
