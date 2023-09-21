using NoPony.Json.JsonTree;
using NoPony.Json.Reflection;
using System;
using System.Collections;
using System.Linq;

namespace NoPony.Json
{
    internal static class Decoder
    {
        internal static object Decode(IJsonObject jsonObject, MetaObject metaObject)
        {
            return decodeObject(jsonObject, metaObject);
        }

        private static object decodeObject(IJsonObject jsonObject, MetaObject metaObject)
        {
            object instance = metaObject.New();

            decodeMemberList(jsonObject, metaObject, instance);

            return instance;
        }

        private static void decodeMemberList(IJsonObject jsonObject, MetaObject metaObject, object instance)
        {
            foreach (IJsonMember jsonMember in jsonObject.Members)
            {
                // If we find a matching member in the type, set its value
                if (metaObject.Members.TryGetValue(jsonMember.Name, out MetaMember metaMember))
                {
                    decodeMember(jsonMember, metaMember, instance);
                }
            }
        }

        private static void decodeMember(IJsonMember jsonMember, MetaMember metaMember, object instance)
        {
            switch (metaMember.Value.ValueType)
            {
                case MetaValueType.Object:
                    decodeMemberObject(jsonMember, metaMember, instance);

                    break;

                case MetaValueType.Array:
                    decodeMemberArray(jsonMember, metaMember, instance);

                    break;

                case MetaValueType.Number:
                    decodeMemberNumber(jsonMember, metaMember, instance);

                    break;
                case MetaValueType.String:
                    decodeMemberString(jsonMember, metaMember, instance);

                    break;

                case MetaValueType.Boolean:
                    decodeMemberLiteral(jsonMember, metaMember, instance);

                    break;
                //case MetaValueType.Value:
                //    decodeMemberValue(jsonMember, metaMember, instance);

                //    break;

                //case MetaValueType.Collection:
                //    decodeMemberCollection(jsonMember, metaMember, instance);

                //    break;

                default: 
                    throw new ArgumentException(nameof(metaMember));
            }
        }

        private static void decodeMemberObject(IJsonMember jsonMember, MetaMember metaMember, object instance)
        {
            metaMember.Set(instance, decodeObject(jsonMember.Value.ObjectValue, metaMember.Value.Type));
        }

        private static void decodeMemberArray(IJsonMember jsonMember, MetaMember metaMember, object instance)
        {
            metaMember.Set(instance,
                jsonMember.Value.ArrayValue.Values
                    .Select(i => decodeObject(i.ObjectValue, metaMember.Value.Type))
                    .ToArray());
        }

        private static void decodeMemberNumber(IJsonMember jsonMember, MetaMember metaMember, object instance)
        {
            metaMember.Set(instance, jsonMember.Value.NumberValue);
        }

        private static void decodeMemberString(IJsonMember jsonMember, MetaMember metaMember, object instance)
        {
            metaMember.Set(instance, jsonMember.Value.StringValue);
        }

        private static void decodeMemberLiteral(IJsonMember jsonMember, MetaMember metaMember, object instance)
        {
switch (jsonMember.Value.LiteralValue)
                    {
                        case JsonLiteral.Null:
                            metaMember.Set(instance, null);
                            break;

                        case JsonLiteral.True:
                            metaMember.Set(instance, true);
                            break;

                        case JsonLiteral.False:
                            metaMember.Set(instance, false);
                            break;

                        default:
                            throw new Exception();
                    }
        }

        // this falls over a bit when we want to set a bool value from the json to, say, an int field
        // or assigning a numeric value from json to a bool or a string or a 'lesser' numeric type (decimal --> int)
        // there will be other such adventures...
        private static void decodeMemberValue(IJsonMember jsonMember, MetaMember metaMember, object instance)
        {
            switch (jsonMember.Value.Type)
            {
                case JsonValueType.Literal:
                    switch (jsonMember.Value.LiteralValue)
                    {
                        case JsonLiteral.Null:
                            metaMember.Set(instance, null);
                            break;

                        case JsonLiteral.True:
                            metaMember.Set(instance, true);
                            break;

                        case JsonLiteral.False:
                            metaMember.Set(instance, false);
                            break;

                        default:
                            throw new Exception();
                    }

                    break;

                case JsonValueType.Number:
                    metaMember.Set(instance, jsonMember.Value.NumberValue);

                    break;

                case JsonValueType.String:
                    metaMember.Set(instance, jsonMember.Value.StringValue);

                    break;
            }
        }


        //private static void decodeMemberCollection(IJsonMember jsonMember, MetaMember metaMember, object instance)
        //{
        //    metaMember.Set(instance,
        //        jsonMember.Value.ArrayValue.Values
        //            .Select(i => decodeObject(i.ObjectValue, metaMember.Value.Type)));
        //}
    }
}
