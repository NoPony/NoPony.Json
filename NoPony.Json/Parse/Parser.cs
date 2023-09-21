using NoPony.Json.JsonTree;
using NoPony.Json.Lex;
using NoPony.Json.ObjectPool;
using System;
using System.Text;

namespace NoPony.Json.Parse
{
    internal class Parser : IPoolable
    {
        private IPool<Parser> _pool;
        private IJsonObject _result;

        // All the pools that are local to this Parser
        // Having them as members like this avoids sharing pools across threads
        private readonly IPool<IJsonArray> _arrayPool;
        private readonly IPool<IJsonMember> _memberPool;
        private readonly IPool<IJsonObject> _objectPool;
        private readonly IPool<IJsonValue> _valuePool;
        private readonly IPool<IJsonMemberList> _memberListPool;
        private readonly IPool<IJsonValueList> _valueListPool;

        // We use a StringBuilder somewhere here, so lets just instantiate one at the member level and reuse it
        private readonly StringBuilder _sb;

        internal Parser(IPool<Parser> Pool)
        {
            _pool = Pool ?? throw new ArgumentNullException("Pool");

            _arrayPool = new Pool<IJsonArray>(4096, p => new JsonArray(p));
            _memberPool = new Pool<IJsonMember>(4096, p => new JsonMember(p));
            _objectPool = new Pool<IJsonObject>(4096, p => new JsonObject(p));
            _valuePool = new Pool<IJsonValue>(4096, p => new JsonValue(p));
            _memberListPool = new Pool<IJsonMemberList>(4096, p => new JsonMemberList(p));
            _valueListPool = new Pool<IJsonValueList>(4096, p => new JsonValueList(p));

            _sb = new StringBuilder();
        }

        public IJsonObject Result { get => _result; }

        public bool Parse(string Source, Lexer Lexer)
        {
            int tokenCursor = 0;

            if (!parseObject(Source, Lexer, ref tokenCursor, out IJsonObject jsonRoot))
            {
                if (jsonRoot != null)
                    jsonRoot.Dispose();

                return false;
            }

            // Ensure that all tokens have been consumed
            if (tokenCursor != Lexer.Count)
                return false;

            _result = jsonRoot;

            return true;
        }

        // <object> ::= '{' <optionalMemberList> '}';
        private bool parseObject(string source, Lexer lexer, ref int tokenCursor, out IJsonObject result)
        {
            if (tokenCursor >= lexer.Count)
            {
                result = null;
                return false;
            }

            int localTokenCursor = tokenCursor;

            if (!parseConcrete(lexer, ref localTokenCursor, TokenType.LiteralStartObject))
            {
                result = null;
                return false;
            }

            if (!parseOptionalMemberList(source, lexer, ref localTokenCursor, out IJsonMemberList members))
            {
                result = null;
                return false;
            }

            if (!parseConcrete(lexer, ref localTokenCursor, TokenType.LiteralEndObject))
            {
                result = null;
                return false;
            }

            result = _objectPool.Aquire();
            result.Members = members;

            tokenCursor = localTokenCursor;

            return true;
        }

        // <optionalMemberList> ::= [<member> <memberListTail>];
        private bool parseOptionalMemberList(string source, Lexer lexer, ref int tokenCursor, out IJsonMemberList result)
        {
            if (tokenCursor >= lexer.Count)
            {
                result = null;
                return false;
            }

            int localTokenCursor = tokenCursor;

            if (parseMember(source, lexer, ref localTokenCursor, out IJsonMember firstMember))
            {
                result = _memberListPool.Aquire();

                result.Add(firstMember);

                parseMemberListTail(source, lexer, ref localTokenCursor, result);

                tokenCursor = localTokenCursor;
            }

            else
            {
                result = null;
            }

            return true;
        }

        // <memberListTail> ::= (',' <member>)*;
        private bool parseMemberListTail(string source, Lexer lexer, ref int tokenCursor, IJsonMemberList result)
        {
            if (tokenCursor >= lexer.Count)
            {
                return false;
            }

            int localTokenCursor = tokenCursor;

            while (true)
            {
                if (!parseConcrete(lexer, ref localTokenCursor, TokenType.LiteralComma))
                    break;

                if (parseMember(source, lexer, ref localTokenCursor, out IJsonMember member))
                    result.Add(member);

                else
                    break;
            }

            tokenCursor = localTokenCursor;

            return true;
        }

        // <array> ::= '[' <optionalValueList> ']';
        private bool parseArray(string source, Lexer lexer, ref int tokenCursor, out IJsonArray result)
        {
            if (tokenCursor >= lexer.Count)
            {
                result = null;
                return false;
            }

            int localTokenCursor = tokenCursor;

            result = null;

            if (!parseConcrete(lexer, ref localTokenCursor, TokenType.LiteralStartArray))
                return false;

            parseOptionalValueList(source, lexer, ref localTokenCursor, out IJsonValueList values);

            if (!parseConcrete(lexer, ref localTokenCursor, TokenType.LiteralEndArray))
            {
                if (values != null)
                    values.Dispose();

                return false;
            }

            result = _arrayPool.Aquire();
            result.Values = values;

            tokenCursor = localTokenCursor;

            return true;
        }

        // <optionalValueList> ::= [<value> <valueListTail>];
        private void parseOptionalValueList(string source, Lexer lexer, ref int tokenCursor, out IJsonValueList result)
        {
            if (tokenCursor >= lexer.Count)
            {
                result = null;
                return;
            }

            int localTokenCursor = tokenCursor;

            if (parseValue(source, lexer, ref localTokenCursor, out IJsonValue firstValue))
            {
                result = _valueListPool.Aquire();

                result.Add(firstValue);

                parseValueListTail(source, lexer, ref localTokenCursor, result);
            }

            else
                result = null;

            tokenCursor = localTokenCursor;

            return;
        }

        // <valueListTail> ::= (',' <value>)*;
        private bool parseValueListTail(string source, Lexer lexer, ref int tokenCursor, IJsonValueList result)
        {
            if (tokenCursor >= lexer.Count)
            {
                return false;
            }

            int localTokenCursor = tokenCursor;

            while (true)
            {
                if (!parseConcrete(lexer, ref localTokenCursor, TokenType.LiteralComma))
                    break;

                if (parseValue(source, lexer, ref localTokenCursor, out IJsonValue value))
                    result.Add(value);

                else
                    break;
            }

            tokenCursor = localTokenCursor;

            return true;
        }

        // <member> ::= <name> ':' <value>;
        private bool parseMember(string source, Lexer lexer, ref int tokenCursor, out IJsonMember result)
        {
            if (tokenCursor >= lexer.Count)
            {
                result = null;
                return false;
            }

            int localTokenCursor = tokenCursor;

            string name = null;

            if (!parseName(source, lexer, ref localTokenCursor, ref name))
            {
                result = null;
                return false;
            }

            if (!parseConcrete(lexer, ref localTokenCursor, TokenType.LiteralColon))
            {
                result = null;
                return false;
            }

            if (!parseValue(source, lexer, ref localTokenCursor, out IJsonValue value))
            {
                result = null;
                return false;
            }

            result = _memberPool.Aquire();
            result.Name = name;
            result.Value = value;

            tokenCursor = localTokenCursor;

            return true;
        }

        // <name> ::= <string>;
        private bool parseName(string source, Lexer lexer, ref int tokenCursor, ref string result)
        {
            if (tokenCursor >= lexer.Count)
            {
                return false;
            }

            int localTokenCursor = tokenCursor;

            if (!parseString(source, lexer, ref localTokenCursor, ref result))
                return false;

            tokenCursor = localTokenCursor;

            return true;
        }

        // <value> ::= <string> | <object> | <array> | <number> | <literal>;
        private bool parseValue(string source, Lexer lexer, ref int tokenCursor, out IJsonValue result)
        {
            if (tokenCursor >= lexer.Count)
            {
                result = null;
                return false;
            }

            int localTokenCursor = tokenCursor;

            switch (lexer.GetAt(tokenCursor).Type)
            {
                case TokenType.String:
                    {
                        string stringResult = null;

                        if (parseString(source, lexer, ref localTokenCursor, ref stringResult))
                        {
                            result = _valuePool.Aquire();
                            result.Type = JsonValueType.String;
                            result.StringValue = stringResult;

                            tokenCursor = localTokenCursor;

                            return true;
                        }

                        break;
                    }

                case TokenType.LiteralStartObject:
                    {
                        if (parseObject(source, lexer, ref localTokenCursor, out IJsonObject objectResult))
                        {
                            result = _valuePool.Aquire();
                            result.Type = JsonValueType.Object;
                            result.ObjectValue = objectResult;

                            tokenCursor = localTokenCursor;

                            return true;

                        }
                        break;
                    }

                case TokenType.LiteralStartArray:
                    {
                        if (parseArray(source, lexer, ref localTokenCursor, out IJsonArray arrayResult))
                        {
                            result = _valuePool.Aquire();
                            result.Type = JsonValueType.Array;
                            result.ArrayValue = arrayResult;

                            tokenCursor = localTokenCursor;

                            return true;
                        }

                        break;
                    }

                case TokenType.Number:
                    {
                        decimal numberResult = 0;

                        if (parseNumber(source, lexer, ref localTokenCursor, ref numberResult))
                        {
                            result = _valuePool.Aquire();
                            result.Type = JsonValueType.Number;
                            result.NumberValue = numberResult;

                            tokenCursor = localTokenCursor;

                            return true;
                        }
                        break;
                    }

                case TokenType.LiteralTrue:
                    result = _valuePool.Aquire();
                    result.Type = JsonValueType.Literal;
                    result.LiteralValue = JsonLiteral.True;

                    tokenCursor++;

                    return true;

                case TokenType.LiteralFalse:
                    result = _valuePool.Aquire();
                    result.Type = JsonValueType.Literal;
                    result.LiteralValue = JsonLiteral.False;

                    tokenCursor++;

                    return true;

                case TokenType.LiteralNull:
                    result = _valuePool.Aquire();
                    result.Type = JsonValueType.Literal;
                    result.LiteralValue = JsonLiteral.Null;

                    tokenCursor++;

                    return true;

                default:
                    break;
            }

            result = null;
            return false;
        }

        // <number> ::= <NUMBER>;
        private bool parseNumber(string source, Lexer lexer, ref int tokenCursor, ref decimal result)
        {
            // TODO: Make this suck less
            Token tempToken = lexer.GetAt(tokenCursor);

            string s = source.Substring(tempToken.Start, tempToken.End - tempToken.Start);

            if (!Decimal.TryParse(s, out result))
                return false;

            tokenCursor++;

            return true;
        }

        // <literal> ::= <TRUE> | <FALSE> | <NULL>
        private bool parseLiteral(string source, Lexer lexer, ref int tokenCursor, ref JsonLiteral result)
        {
            switch (lexer.GetAt(tokenCursor).Type)
            {
                case TokenType.LiteralTrue:
                    result = JsonLiteral.True;

                    tokenCursor++;

                    return true;

                case TokenType.LiteralFalse:
                    result = JsonLiteral.False;

                    tokenCursor++;

                    return true;

                case TokenType.LiteralNull:
                    result = JsonLiteral.Null;

                    tokenCursor++;

                    return true;

                default:
                    return false;
            }
        }

        // <string> ::= <STRING>;
        private bool parseString(string source, Lexer lexer, ref int tokenCursor, ref string result)
        {
            Token t = lexer.GetAt(tokenCursor);

            if (t.Type == TokenType.String)
                result = source.Substring(t.Start, t.End - t.Start);

            else if (t.Type == TokenType.StringEscape)
                result = escapeString(source.Substring(t.Start, t.End - t.Start));

            else
                return false;

            tokenCursor++;

            return true;
        }

        private string escapeString(string source)
        {
            _sb.Clear();

            int sourceCursor = 0;
            int endCursor = source.Length;

            while (sourceCursor < endCursor)
            {
                char c = source[sourceCursor++];

                if (c == '\\')
                {
                    switch (source[sourceCursor++])
                    {
                        case '\"':
                            _sb.Append('\"');
                            break;

                        case '\\':
                            _sb.Append('\\');
                            break;

                        case '/':
                            _sb.Append('/');
                            break;

                        case 'b':
                            _sb.Append('\b');
                            break;

                        case 'f':
                            _sb.Append('\f');
                            break;

                        case 'n':
                            _sb.Append('\n');
                            break;

                        case 'r':
                            _sb.Append('\r');
                            break;

                        case 't':
                            _sb.Append('\t');
                            break;

                        case 'u': // XXXX(4 hex digits)
                            string h = source.Substring(sourceCursor, 4);

                            if (int.TryParse(h, System.Globalization.NumberStyles.HexNumber, null, out int i))
                            {
                                _sb.Append((char)i);
                            }

                            _sb.Append('\"');

                            break;

                        default: // just drop any unrecognized escapes
                            break;
                    }
                }

                else
                {
                    _sb.Append(c);
                }
            }

            return _sb.ToString();
        }

        private bool parseConcrete(Lexer lexer, ref int tokenCursor, TokenType tokenType)
        {
            if (lexer.GetAt(tokenCursor).Type != tokenType)
                return false;

            tokenCursor++;

            return true;
        }

        // IPoolable
        public void Clear()
        {
            _result?.Dispose();
            _result = null;
        }

        public void Dispose()
        {
            _result?.Dispose();
            _result = null;

            _pool?.Release(this);
        }
    }
}
