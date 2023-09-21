using NoPony.Json.ObjectPool;
using System;

namespace NoPony.Json.Lex
{
    // what I wouldn't give for the source to the lexer I made at Sparx..
    internal class Lexer : IPoolable
    {
        private readonly IPool<Lexer> _pool;
        private TokenBuffer _result;

        internal Lexer(IPool<Lexer> Pool)
        {
            _pool = Pool ?? throw new ArgumentNullException("Pool");
            _result = new TokenBuffer();
        }

        internal bool Parse(string Input)
        {
            _result.Clear();

            return lex(Input);
        }

        internal Token GetAt(int Index)
        {
            return _result.GetAt(Index);
        }

        internal int Count { get => _result.Count; }

        private bool lex(string input)
        {
            int cursor = 0;

            // Keep lexing a token at a time until lexToken fails
            // A failure at end of string is expected
            while (lexToken(input, ref cursor))
            {
                // A while loop, with no body; what is this madness?
            }

            // Fail if the entire string has not been consumed
            if (cursor != input.Length)
                return false;

            return true;
        }

        // <TOKENS>						            ::= <WHITESPACE> |
        //									            <NUMBER> |
        //									            "\"" (<STRING> | <STRING_ESCAPE>) "\"" |
        //									            <LITERAL>;
        private bool lexToken(string input, ref int cursor)
        {
            if (cursor >= input.Length)
                return false;

            int localCursor = cursor;

            if (lexWhitespace(input, ref localCursor))
            {
                cursor = localCursor;

                return true;
            }

            if (lexLiteral(input, ref localCursor))
            {
                cursor = localCursor;

                return true;
            }

            if (lexString(input, ref localCursor))
            {
                cursor = localCursor;

                return true;
            }

            if (lexNumber(input, ref localCursor))
            {
                cursor = localCursor;

                return true;
            }

            return false;
        }

        // <WHITESPACE>							    ::= " " | "\r" | "\n" | "\t";
        private bool lexWhitespace(string input, ref int cursor)
        {
            if (cursor >= input.Length)
                return false;

            int localCursor = cursor;

            switch (input[localCursor++])
            {
                case ' ': break;
                case '\r': break;
                case '\n': break;
                case '\t': break;
                default: return false;
            }

            cursor = localCursor;

            return true;
        }

        // <NUMBER>								::= ["-"] ("0" | "1".."9" "0".."9"*) ["." "0".."9"+] [("e" | "E") ["+" | "-"] "0".."9"+];
        private bool lexNumber(string input, ref int cursor)
        {
            if (cursor >= input.Length)
                return false;

            int localCursor = cursor;

            // ["-"] 
            lexConcrete(input, ref localCursor, '-');

            // ("0" | "1".."9" "0".."9"*) 
            if (lexConcrete(input, ref localCursor, '0'))
            { }

            else if (lexConcreteRange(input, ref localCursor, '1', '9'))
                while (lexConcreteRange(input, ref localCursor, '0', '9')) ;

            else
                return false;

            // ["." "0".."9"+]
            if (lexConcrete(input, ref localCursor, '.'))
            {
                if (!lexConcreteRange(input, ref localCursor, '0', '9'))
                    return false;

                while (lexConcreteRange(input, ref localCursor, '0', '9')) ;
            }

            // [("e" | "E") ["+" | "-"] "0".."9"+]
            if (lexConcrete(input, ref localCursor, 'e') || lexConcrete(input, ref localCursor, 'E'))
            {
                if (lexConcrete(input, ref localCursor, '+'))
                { }

                else if (lexConcrete(input, ref localCursor, '-'))
                { }

                if (!lexConcreteRange(input, ref localCursor, '0', '9'))
                    return false;

                while (lexConcreteRange(input, ref localCursor, '0', '9')) ;
            }

            _result.Add(cursor, localCursor, TokenType.Number);

            cursor = localCursor;

            return true;
        }

        // <STRING>								::= token("\'") token(skip("\'", "\\\'")) token("\'") |
        //                                          token("\"") token(skip("\"", "\\\"")) token("\"");
        private bool lexString(string input, ref int cursor)
        {
            return lexStringDouble(input, ref cursor);
        }

        //private bool lexString(ITokenBuffer result, string input, ref int cursor)
        //{
        //    if (cursor >= input.Length)
        //        return false;

        //    int localCursor = cursor;

        //    if (lexStringSingle(result, input, ref localCursor))
        //    {
        //        cursor = localCursor;

        //        return true;
        //    }

        //    if (lexStringDouble(result, input, ref localCursor))
        //    {
        //        cursor = localCursor;

        //        return true;
        //    }

        //    return false;
        //}

        //private bool lexStringSingle(ITokenBuffer result, string input, ref int cursor)
        //{
        //    if (cursor >= input.Length)
        //        return false;

        //    // We make a token of the string body, not including the quotes
        //    int localCursor = cursor;

        //    if (!lexLiteral(input, ref localCursor, '\''))
        //        return false;

        //    int tokenStart = localCursor;

        //    // **FIX: Need to allow for escape in lexSkip
        //    if (!lexSkip(input, ref localCursor, '\''))
        //        return false;

        //    int tokenEnd = localCursor;

        //    if (!lexLiteral(input, ref localCursor, '\''))
        //        return false;

        //    result.Add(tokenStart, tokenEnd, TokenType.String);

        //    cursor = localCursor;

        //    return true;
        //}

        private bool lexStringDouble(string input, ref int cursor)
        {
            if (cursor >= input.Length)
                return false;

            // We make a token of the string body, not including the quotes; I don't want to have to strip them out later
            int localCursor = cursor;

            if (!lexConcrete(input, ref localCursor, '\"'))
                return false;

            int tokenStart = localCursor;

            // TODO: Need to identify strings with escapes so they can be handled better by the parser
            // given that this is the only place lexSkip is called, it might be easiest to inline in here
            // that way i don't need to pass back a 'string had escapes' flag
            //if (!lexSkip(input, ref localCursor, '\"', lexStringDoubleEscape))
            //    return false;

            bool escape = false;

            // Until end of string
            while (localCursor < input.Length)
            {
                // If we match the target, set the cursor to point at the target and return success
                if (input[localCursor] == '\"')
                {
                    //cursor = localCursor;

                    //return true;
                    break;
                }

                // String escapes always start with a backslash
                // TODO: Not sure that passing in a skip method helps me.  String escapes wont change, and the extra call is overhead I don't need
                // Maybe manually inline it?
                // overhead includes creating a temp cursor as well as the method call
                if (input[localCursor] == '\\')
                {
                    escape = true;
                    lexStringDoubleEscape(input, ref localCursor);
                }

                // Increment our local cursor
                localCursor++;
            }




            int tokenEnd = localCursor;

            if (!lexConcrete(input, ref localCursor, '\"'))
                return false;

            if (escape)
                _result.Add(tokenStart, tokenEnd, TokenType.StringEscape);

            else
                _result.Add(tokenStart, tokenEnd, TokenType.String);

            cursor = localCursor;

            return true;
        }

        private void lexStringDoubleEscape(string input, ref int cursor)
        {
            int localCursor = cursor;

            if (input[localCursor++] == '\\')
            {
                switch (input[localCursor++])
                {
                    case '\"':
                        cursor = localCursor;
                        break;

                    case '\\':
                        cursor = localCursor;
                        break;

                    case '/':
                        cursor = localCursor;
                        break;

                    case 'b':
                        cursor = localCursor;
                        break;

                    case 'f':
                        cursor = localCursor;
                        break;

                    case 'n':
                        cursor = localCursor;
                        break;

                    case 'r':
                        cursor = localCursor;
                        break;

                    case 't':
                        cursor = localCursor;
                        break;

                    case 'u': /// XXXX(4 hex digits)
                        // TODO: Make this suck less
                        // these are all ascii values, so i can create a lookup in an array
                        char c = input[localCursor++];

                        if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
                            break;

                        c = input[localCursor++];

                        if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
                            break;

                        c = input[localCursor++];

                        if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
                            break;

                        c = input[localCursor++];

                        if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
                            break;

                        cursor = localCursor;

                        break;
                }
            }
        }

        private bool lexLiteral(string input, ref int cursor)
        {
            if (cursor >= input.Length)
                return false;

            int localCursor = cursor;

            switch (input[localCursor])
            {
                case 't':
                    if (lexConcrete(input, ref localCursor, "true"))
                        _result.Add(TokenType.LiteralTrue);

                    else
                        return false;

                    break;

                case 'f':
                    if (lexConcrete(input, ref localCursor, "false"))
                        _result.Add(TokenType.LiteralFalse);

                    else
                        return false;

                    break;

                case 'n':
                    if (lexConcrete(input, ref localCursor, "null"))
                        _result.Add(TokenType.LiteralNull);

                    else
                        return false;

                    break;

                case '{':
                    _result.Add(TokenType.LiteralStartObject);
                    localCursor++;

                    break;

                case '}':
                    _result.Add(TokenType.LiteralEndObject);
                    localCursor++;

                    break;

                case '[':
                    _result.Add(TokenType.LiteralStartArray);
                    localCursor++;

                    break;

                case ']':
                    _result.Add(TokenType.LiteralEndArray);
                    localCursor++;

                    break;

                case ',':
                    _result.Add(TokenType.LiteralComma);
                    localCursor++;

                    break;

                case ':':
                    _result.Add(TokenType.LiteralColon);
                    localCursor++;

                    break;

                default:
                    return false;
            }

            cursor = localCursor;

            return true;
        }

        //delegate void LexSkipEscape(string input, ref int cursor);

        // Skip all chars until the specified target char is found
        //private bool lexSkip(string input, ref int cursor, char target, LexSkipEscape lexSkipEscape)
        //{
        //    // Fail on input past end
        //    if (cursor >= input.Length)
        //        return false;

        //    // Create local cursor
        //    int localCursor = cursor;

        //    // Until end of string
        //    while (localCursor < input.Length)
        //    {
        //        // If we match the target, set the cursor to point at the target and return success
        //        if (input[localCursor] == target)
        //        {
        //            cursor = localCursor;

        //            return true;
        //        }

        //        // String escapes always start with a backslash
        //        // TODO: Not sure that passing in a skip method helps me.  String escapes wont change, and the extra call is overhead I don't need
        //        // Maybe manually inline it?
        //        // overhead includes creating a temp cursor as well as the method call
        //        if (input[localCursor] == '\\')
        //            lexSkipEscape(input, ref localCursor);

        //        // Increment our local cursor
        //        localCursor++;
        //    }

        //    return false;
        //}

        // TODO: This skip routine needs to support a escapes; see above
        //
        // ** Or it might, if it was actually called anywhere
        //private bool lexSkip(string input, ref int cursor, string target)
        //{
        //    if (cursor >= input.Length)
        //        return false;

        //    int localCursor = cursor;

        //    while (localCursor < input.Length)
        //    {
        //        // we use junkLocalCursor here instead of localCursor so lexLiteral doesn't consume the current (target) chars
        //        // if we get a match, we return the cursor of the start of current (matched target) chars
        //        int junkLocalCursor = localCursor;

        //        if (lexLiteral(input, ref junkLocalCursor, target))
        //        {
        //            cursor = localCursor;

        //            return true;
        //        }

        //        localCursor++;
        //    }

        //    return false;
        //}

        //private static bool lexSkipEOF(string input, ref int cursor)
        //{
        //    if (cursor >= input.Length)
        //        return false;

        //    cursor = input.Length;

        //    return true;
        //}

        private static bool lexConcrete(string input, ref int cursor, char value)
        {
            if (cursor >= input.Length)
                return false;

            int localCursor = cursor;

            if (input[localCursor++] != value)
                return false;

            cursor = localCursor;

            return true;
        }

        private static bool lexConcrete(string input, ref int cursor, string value)
        {
            if (cursor >= input.Length)
                return false;

            int localCursor = cursor;

            for (int i = 0; i < value.Length; i++)
            {
                if (localCursor >= input.Length)
                    return false;

                if (input[localCursor] != value[i])
                    return false;
            }

            cursor = localCursor;

            return true;
        }

        private static bool lexConcreteRange(string input, ref int cursor, char startValue, char endValue)
        {
            if (cursor >= input.Length)
                return false;

            int localCursor = cursor;

            char inputValue = input[localCursor++];

            if (inputValue < startValue || inputValue > endValue)
                return false;

            cursor = localCursor;

            return true;
        }

        // IPoolable
        public void Clear()
        {
            _result.Clear();
        }

        public void Dispose()
        {
            _pool.Release(this);
        }
    }
}
