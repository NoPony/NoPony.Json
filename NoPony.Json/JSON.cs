using NoPony.Json.JsonTree;
using NoPony.Json.Lex;
using NoPony.Json.ObjectPool;
using NoPony.Json.Parse;
using NoPony.Json.Reflection;
using NoPony.Json.Translation;
using System;
using System.Collections.Generic;

namespace NoPony.Json
{
    public static class JSON
    {
        private static IPool<Lexer> _lexerPool = new Pool<Lexer>(32, pool => new Lexer(pool));
        private static IPool<Parser> _parserPool = new Pool<Parser>(32, pool => new Parser(pool));
        private static IPool<Encoder> _encoderPool = new Pool<Encoder>(32, pool => new Encoder(pool));

        private static IPool<JsonResult> _jsonResultPool = new Pool<JsonResult>(32, pool => new JsonResult(pool));

        private static Dictionary<string, MetaObject> _metaCache = new Dictionary<string, MetaObject>();

        public static string Serialize<T>(T Instance)
        {
            Type type = typeof(T);

            return Serialize(type, Instance);
        }

        public static string Serialize(Type Type, object Instance)
        {
            if (!_metaCache.TryGetValue(Type.AssemblyQualifiedName, out MetaObject metaObject))
            { }

            using (Encoder encoder = _encoderPool.Aquire())
            {
                return encoder.Encode(Instance, metaObject);
            }
        }

        public static IJsonObject Deserialize(string Input)
        {
            using (Lexer lexer = _lexerPool.Aquire())
            {
                lexer.Parse(Input);

                // no using here, we don't want to dispose of parser yet
                // parser gets dispised by JsonResult when it gets disposed by the caller
                Parser parser = _parserPool.Aquire();

                parser.Parse(Input, lexer);

                JsonResult result = _jsonResultPool.Aquire();

                result.Parser = (IParser)parser;

                return result;
            }
        }

        public static T Deserialize<T>(string Input) where T : class, new()
        {
            using (Lexer lexer = _lexerPool.Aquire())
            {
                lexer.Parse(Input);

                using (Parser parser = _parserPool.Aquire())
                {
                    parser.Parse(Input, lexer);

                    MetaObject metaObject = _metaCache[typeof(T).Name];

                    return (T)Decoder.Decode(parser.Result, metaObject);
                }
            }
        }
    }
}
