using NoPony.Json.ObjectPool;
using System;

namespace NoPony.Json.Lex
{
    internal class TokenBuffer
    {
        private const int TOKEN_BUFFER_DEFUALT_SIZE = 1024;

        private Token[] _buffer;
        private int _bufferSize;
        private int _itemCount;

        internal TokenBuffer() : this(TOKEN_BUFFER_DEFUALT_SIZE)
        {
        }

        internal TokenBuffer(int InitialSize)
        {
            _buffer = new Token[InitialSize];
            _bufferSize = InitialSize;

            _itemCount = 0;
        }

        public int Count
        {
            get
            {
                return _itemCount;
            }
        }

        public void Clear()
        {
            _itemCount = 0;
        }

        public void Add(TokenType Type)
        {
            if (_itemCount >= _bufferSize)
            {
                _bufferSize += TOKEN_BUFFER_DEFUALT_SIZE;
                resize(_bufferSize);
            }

            _buffer[_itemCount].Type = Type;

            _itemCount++;
        }

        public void Add(int Start, int End, TokenType Type)
        {
            if (_itemCount >= _bufferSize)
            {
                _bufferSize += TOKEN_BUFFER_DEFUALT_SIZE;
                resize(_bufferSize);
            }

            _buffer[_itemCount].Start = Start;
            _buffer[_itemCount].End = End;
            _buffer[_itemCount].Type = Type;

            _itemCount++;
        }

        public Token GetAt(int index)
        {
            if (index >= _itemCount)
                throw new Exception();

            return _buffer[index];
        }

        private void resize(int newSize)
        {
            // TODO: This is an abomination
            Array.Resize(ref _buffer, newSize);
        }
    }
}
