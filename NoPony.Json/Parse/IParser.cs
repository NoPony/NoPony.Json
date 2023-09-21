using NoPony.Json.JsonTree;
using NoPony.Json.Lex;
using System;

namespace NoPony.Json.Parse
{
    internal interface IParser : IDisposable
    {
        bool Parse(string Source, Lexer Lexer);
        IJsonObject Result { get; }
    }
}
