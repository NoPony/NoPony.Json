using NoPony.Json.JsonTree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NoPony.Json.Reflection
{
    internal class MetaInspector
    {
        internal MetaObject Inspect(Type Type)
        {
            return inspectObject(Type);
        }

        private MetaObject inspectObject(Type type)
        {
            return new MetaObject
            {
                Name = type.FullName,
                Members = inspectMemberList(type).ToDictionary(i => i.Name),
                New = compileConstructor(type),
            };
        }

        private IEnumerable<MetaMember> inspectMemberList(Type type)
        {
            return Enumerable.Union
            (
                type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).Cast<MemberInfo>(),
                type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).Cast<MemberInfo>()
            )
            .Select(i => inspectMember(i));
        }

        private MetaMember inspectMember(MemberInfo member)
        {
            switch (inspectMemberValueType(member.GetType()))
            {
                case MetaValueType.Object: return inspectMemberObject(member);
                case MetaValueType.Array: return inspectMemberArray(member);
                case MetaValueType.Collection: return inspectMemberCollection(member);
                case MetaValueType.CollectionT: return inspectMemberGenericCollection(member);
                case MetaValueType.Number: return inspectMemberNumber(member);
                case MetaValueType.String: return inspectMemberString(member);
                case MetaValueType.Boolean: return inspectMemberBoolean(member);
                default: throw new Exception();
            }
        }

        private MetaMember inspectMemberObject(MemberInfo member)
        {
            return new MetaMember
            {
                Value = new MetaValue
                {
                    ValueType = MetaValueType.Object,
                    Type = inspectObject(member.GetType()),
                },
                Name = member.Name,
                Get = compileGetter(member),
                Set = compileSetter(member),
            };
        }

        private MetaMember inspectMemberArray(MemberInfo member)
        {
            var memberType = member.GetType();
            var elementType = memberType.GetElementType();

            return new MetaMember
            {
                Name = member.Name,
                Value = new MetaValue
                {
                    ValueType = MetaValueType.Array,
                    Type = inspectObject(elementType),
                    ContainerValue = new MetaContainer
                    {
                        //NewContainer = compileArrayInit(memberType),
                        NewContainerItem = compileConstructor(elementType),
                    },
                },

                Get = compileGetter(member),
                Set = compileSetter(member),
            };
        }

        private MetaMember inspectMemberCollection(MemberInfo member)
        {
            return new MetaMember
            {
                Value = new MetaValue
                {
                    ValueType = MetaValueType.Array,
                    Type = inspectObject(member.GetType()),
                },
                Name = member.Name,

                Get = compileGetter(member),
                Set = compileSetter(member),
            };
        }

        private MetaMember inspectMemberGenericCollection(MemberInfo member)
        {
            var type = member.GetType();

            return new MetaMember
            {
                Name = member.Name,
                Value = new MetaValue
                {
                    ValueType = MetaValueType.CollectionT,
                    Type = inspectObject(type),
                    ContainerValue = new MetaContainer
                    {
                        NewContainer = compileGenericConstructor(type),
                        NewContainerItem = compileConstructor(type.GetGenericTypeDefinition().GetGenericArguments().Single()),
                    },
                },

                Get = compileGetter(member),
                Set = compileSetter(member),
            };
        }

        private MetaMember inspectMemberNumber(MemberInfo member)
        {
            return new MetaMember
            {
                Name = member.Name,
                Value = new MetaValue
                {
                    ValueType = MetaValueType.Number,
                },

                Get = compileGetter(member),
                Set = compileSetter(member),
            };
        }

        private MetaMember inspectMemberString(MemberInfo member)
        {
            return new MetaMember
            {
                Name = member.Name,
                Value = new MetaValue
                {
                    ValueType = MetaValueType.String,
                },

                Get = compileGetter(member),
                Set = compileSetter(member),
            };
        }

        private MetaMember inspectMemberBoolean(MemberInfo member)
        {
            return new MetaMember
            {
                Name = member.Name,
                Value = new MetaValue
                {
                    ValueType = MetaValueType.Boolean,
                },

                Get = compileGetter(member),
                Set = compileSetter(member),
            };
        }

        //private MetaMember inspectMemberCollection(MemberInfo member)
        //{
        //    return new MetaMember
        //    {
        //        Name = member.Name,
        //        Value=new MetaValue
        //        {
        //            ValueType = MetaValueType.Collection,
        //            Type = inspectObject(member.GetType()),
        //        },

        //        Get = compileGetter(member),
        //        Set = compileSetter(member),
        //    };
        //}

        private Func<object, object> compileGetter(MemberInfo member)
        {
            ParameterExpression paramInstance = Expression.Parameter(typeof(object));
            UnaryExpression instance = Expression.Convert(paramInstance, member.GetType().DeclaringType);

            MemberExpression paramProperty = Expression.PropertyOrField(instance, member.Name);
            UnaryExpression property = Expression.Convert(paramProperty, member.GetType());


            UnaryExpression boxedProperty = Expression.Convert(property, typeof(object));

            return Expression
                .Lambda<Func<object, object>>(property, paramInstance)
                .Compile();
        }

        private Action<object, object> compileSetter(MemberInfo member)
        {
            ParameterExpression paramInstance = Expression.Parameter(member.DeclaringType);
            ParameterExpression paramValue = Expression.Parameter(member.GetType());

            MemberExpression memberAccess = Expression.MakeMemberAccess(paramInstance, member);
            BinaryExpression assign = Expression.Assign(memberAccess, paramValue);

            return Expression
                .Lambda<Action<object, object>>(assign, paramInstance, paramValue)
                .Compile();
        }

        private Func<object> compileConstructor(Type type)
        {
            NewExpression instance = Expression.New(typeof(object));
            UnaryExpression boxedInstance = Expression.Convert(instance, typeof(object));

            return Expression.Lambda<Func<object>>(instance).Compile();
        }

        private Func<object> compileGenericConstructor(Type type)
        {
            Type generic = type.MakeGenericType(type.GetGenericTypeDefinition().GetGenericArguments());

            NewExpression instance = Expression.New(generic);
            UnaryExpression boxedInstance = Expression.Convert(instance, typeof(object));

            return Expression.Lambda<Func<object>>(instance).Compile();
        }

        private Func<object, object> compileArrayInit(Type type)
        {
            var paramItems = Expression.Parameter(typeof(object));
            var items = Expression.Convert(paramItems, type);

            var itemsAccess = Expression.MakeMemberAccess(items, type);
            var a = Expression.Invoke(itemsAccess);

            //var elementType = type.GetElementType();
            //var array = Expression.NewArrayBounds(
            //    elementType,
            //    Expression.Constant(3),
            //    Expression.Constant(2));
            return null;// Expression.Lambda<Func<object>>(a).Compile();
        }

        private MetaValueType inspectMemberValueType(Type type)
        {
            if (isObject(type))
                return MetaValueType.Object;

            if (isArray(type))
                return MetaValueType.Array;

            if (isCollection(type))
                return MetaValueType.Collection;

            if (isCollectionT(type))
                return MetaValueType.CollectionT;

            if (isNumber(type))
                return MetaValueType.Number;

            if (isString(type))
                return MetaValueType.String;

            if (isBoolean(type))
                return MetaValueType.Boolean;

            throw new Exception();
        }

        private bool isObject(Type type)
        {
            return type.IsClass && type.GetConstructor(Type.EmptyTypes) is null;
        }

        private bool isArray(Type type)
        {
            return type.IsArray;
        }

        private bool isCollection(Type type)
        {
            return type.GetInterfaces()
                .Where(i => !i.IsGenericType)
                .Where(i => i == typeof(ICollection))
                .Any();
        }

        private bool isCollectionT(Type type)
        {
            return type.GetInterfaces()
                .Where(i => i.IsGenericType)
                .Where(i => i.GetGenericTypeDefinition() == typeof(ICollection<>))
                .Where(i => i.GetGenericTypeDefinition().GetGenericArguments().Length == 1)
                .Any();
        }

        private bool isEnumerable(Type type)
        {
            return type.GetInterfaces()
                .Where(i => !i.IsGenericType)
                .Where(i => i == typeof(IEnumerable))
                .Any();
        }

        private bool isEnumerableT(Type type)
        {
            return type.GetInterfaces()
                .Where(i => i.IsGenericType)
                .Where(i => i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Where(i => i.GetGenericTypeDefinition().GetGenericArguments().Length == 1)
                .Any();
        }

        private bool isNumber(Type type)
        {
            //return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal);
            return _numberTypeIndex.ContainsKey(type.FullName);
        }

        private bool isString(Type type)
        {
            //return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal);
            return _stringTypeIndex.ContainsKey(type.FullName);
        }

        private bool isBoolean(Type type)
        {
            return type == typeof(bool);
        }

        private bool isEnum(Type type)
        {
            return type.IsEnum;
        }

        //private bool isValue(Type type)
        //{
        //    return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal);
        //}

        private Dictionary<string, bool> _numberTypeIndex = new[]
        {
            new KeyValuePair<string, bool>("System.Byte", true),
            new KeyValuePair<string, bool>("System.SByte", true),
            new KeyValuePair<string, bool>("System.Decimal", true),
            new KeyValuePair<string, bool>("System.Double", true),
            new KeyValuePair<string, bool>("System.Single", true),
            new KeyValuePair<string, bool>("System.Int32", true),
            new KeyValuePair<string, bool>("System.UInt32", true),
            new KeyValuePair<string, bool>("System.Int64", true),
            new KeyValuePair<string, bool>("System.UInt64", true),
            new KeyValuePair<string, bool>("System.Int16", true),
            new KeyValuePair<string, bool>("System.UInt16", true),
        }
        .ToDictionary(i => i.Key, i => i.Value);

        private Dictionary<string, bool> _stringTypeIndex = new[]
        {
            new KeyValuePair<string, bool>("System.Char", true),
            new KeyValuePair<string, bool>("System.String", true),
            new KeyValuePair<string, bool>("System.Object", true),
        }
        .ToDictionary(i => i.Key, i => i.Value);
    }
}
