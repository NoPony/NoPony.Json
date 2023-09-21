using System;
using System.Collections.Generic;
using System.Text;

namespace NoPony.Json.Lex
{
    internal interface ITokenBuffer : IDisposable
    {
        int Count { get; }
        void Clear();
        void Add(TokenType Type);
        void Add(int Start, int End, TokenType Type);
        Token GetAt(int index);
    }
}
