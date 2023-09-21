namespace NoPony.Json.Lex
{
    // TODO: We're mixing token type and literal token values up here
    // Although it's convenient, is it the best way?
    internal enum TokenType
    {
        Number,
        String,
        StringEscape,
        Keyword,

        LiteralTrue,
        LiteralFalse,
        LiteralNull,
        LiteralStartObject,
        LiteralEndObject,
        LiteralStartArray,
        LiteralEndArray,
        LiteralComma,
        LiteralColon,
        LiteralSingleQuote,
        LiteralDoubleQuote,
        LiteralBackslash,
    }
}
