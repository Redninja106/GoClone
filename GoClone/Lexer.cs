using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone;
internal class Lexer
{
    public static Token ReadToken(SourceReader reader)
    {
        char c = reader.Next();
        if (c == '\0')
        {
            return Token.EndOfSource;
        }

        if (c == '\n')
        {
            return reader.MakeToken(TokenKind.NewLine);
        }

        if (char.IsWhiteSpace(c))
        {
            reader.Skip();
            return ReadToken(reader);
        }

        if (char.IsLetter(c))
        {
            while (char.IsLetterOrDigit(reader.Current))
            {
                reader.Next();
            }

            Token tok = reader.MakeToken(TokenKind.Identifier);
            tok.kind = tok.Value switch
            {
                "func" => TokenKind.Func,
                "type" => TokenKind.Type,
                "interface" => TokenKind.Interface,
                "struct" => TokenKind.Struct,
                "int" => TokenKind.Int,
                "bool" => TokenKind.Bool,
                "module" => TokenKind.Module,
                "if" => TokenKind.If,
                "else" => TokenKind.Else,
                "while" => TokenKind.While,
                "var" => TokenKind.Var,
                "for" => TokenKind.For,
                "in" => TokenKind.In,
                "return" => TokenKind.Return,
                "true" => TokenKind.True,
                "false" => TokenKind.False,
                "as" => TokenKind.As,
                "null" => TokenKind.Null,
                "sizeof" => TokenKind.SizeOf,
                "any" => TokenKind.Any,
                "import" => TokenKind.Import,
                "operator" => TokenKind.Operator,
                _ => tok.kind
            };
            return tok;
        }
        if (char.IsDigit(c))
        {
            while (char.IsDigit(reader.Current))
            {
                reader.Next();
            }

            return reader.MakeToken(TokenKind.Number);
        }
        
        switch (c)
        {
            case '(':
                return reader.MakeToken(TokenKind.OpenParenthesis);
            case ')':
                return reader.MakeToken(TokenKind.CloseParenthesis);
            case '[':
                return reader.MakeToken(TokenKind.OpenBracket);
            case ']':
                return reader.MakeToken(TokenKind.CloseBracket);
            case '{':
                return reader.MakeToken(TokenKind.OpenBrace);
            case '}':
                return reader.MakeToken(TokenKind.CloseBrace);
            case ',':
                return reader.MakeToken(TokenKind.Comma);
            case '.':
                return reader.MakeToken(TokenKind.Dot);
            case '-':
                if (reader.Next('>'))
                {
                    return reader.MakeToken(TokenKind.Arrow);
                }
                if (reader.Next('-'))
                {
                    return reader.MakeToken(TokenKind.MinusMinus);
                }
                return reader.MakeToken(TokenKind.Minus);
            case '+':
                if (reader.Next('+'))
                {
                    return reader.MakeToken(TokenKind.PlusPlus);
                }
                return reader.MakeToken(TokenKind.Plus);
            case '%':
                return reader.MakeToken(TokenKind.Percent);
            case '*':
                return reader.MakeToken(TokenKind.Star);
            case '/':
                if (reader.Next('*'))
                {
                    while (!(reader.Peek(-2) == '*' && reader.Peek(-1) == '/'))
                    {
                        reader.Next();
                    }
                    reader.Skip();
                    return ReadToken(reader);
                }
                if (reader.Next('/'))
                {
                    while (reader.Peek(-1) is not ('\n' or '\0'))
                    {
                        reader.Next();
                    }
                    reader.Skip();
                    return ReadToken(reader);
                }

                return reader.MakeToken(TokenKind.Slash);
            case '<':
                if (reader.Next('='))
                {
                    return reader.MakeToken(TokenKind.LessThanEqual);
                }
                return reader.MakeToken(TokenKind.LessThan);
            case '>':
                if (reader.Next('='))
                {
                    return reader.MakeToken(TokenKind.GreaterThanEqual);
                }
                return reader.MakeToken(TokenKind.GreaterThan);
            case '!':
                if (reader.Next('='))
                {
                    return reader.MakeToken(TokenKind.NotEqual);
                }
                return reader.MakeToken(TokenKind.Not);
            case '=':
                if (reader.Next('='))
                {
                    return reader.MakeToken(TokenKind.EqualEqual);
                }
                return reader.MakeToken(TokenKind.Equal);
            case '|':
                if (reader.Next('|'))
                {
                    return reader.MakeToken(TokenKind.OrOr);
                }
                return reader.MakeToken(TokenKind.Or);
            case '&':
                if (reader.Next('&'))
                {
                    return reader.MakeToken(TokenKind.AndAnd);
                }
                return reader.MakeToken(TokenKind.And);
            case ':':
                return reader.MakeToken(TokenKind.Colon);
            case ';':
                return reader.MakeToken(TokenKind.Semicolon);
            case '"':
                while (!reader.Next('"') && reader.Peek(-1) != '\\')
                {
                    reader.Next();
                }
                return reader.MakeToken(TokenKind.String);
            case '\'':
                while (!reader.Next('\'') && reader.Peek(-1) != '\\')
                {
                    if (reader.Next() == 0)
                        break;
                }
                return reader.MakeToken(TokenKind.Character);
            default:
                break;
        }

        return reader.MakeToken(TokenKind.Unknown);
    }
}

class SourceReader(string source)
{
    string source = source;
    int lastTokenPos;
    int position;

    public char Current => Peek(0);

    public char Peek(int offset = 1)
    {
        int index = position + offset;
        if (index < 0 || index >= source.Length)
        {
            return '\0';
        }
        return source[index];
    }

    public char Next()
    {
        char result = Peek(0);
        position++;
        return result;
    }

    public bool Next(char value)
    {
        if (Current == value)
        {
            Next();
            return true;
        }

        return false;
    }

    public void Skip()
    {
        MakeToken(0);
    }

    public Token MakeToken(TokenKind kind)
    {
        Token result = new Token
        {
            start = lastTokenPos,
            end = position,
            kind = kind,
            source = source,
        };

        lastTokenPos = position;
        return result;
    }
}

struct Token
{
    public static readonly Token Unknown = new Token
    {
        start = 0,
        end = 0,
        source = "",
        kind = TokenKind.Unknown,
    };

    public static readonly Token EndOfSource = new Token
    {
        start = 0,
        end = 0,
        source = "",
        kind = TokenKind.EndOfSource
    };

    public int start, end;
    public TokenKind kind;
    public string source;
    public bool hasLineTerminator;

    public readonly ReadOnlySpan<char> Value => start < source.Length ? source.AsSpan(start, end - start) : [];

    public override readonly string ToString()
    {
        return Value.ToString();
    }
}

enum TokenKind
{
    Unknown,
    EndOfSource,
    NewLine,

    Identifier,
    Number,

    String,
    Character,

    // KEYWORDS
    Func,
    Return,
    Type,
    Interface,
    Struct,
    Module,
    If,
    Else,
    While,
    Var,
    For,
    In,
    True,
    False,
    As,
    Null,
    SizeOf,
    Any,
    Import,
    Operator,

    // TYPES

    Int,
    Bool,

    // SYMBOLS

    OpenBracket,
    CloseBracket,
    OpenBrace,
    CloseBrace,
    OpenParenthesis,
    CloseParenthesis,
    And,
    AndAnd,
    Or,
    OrOr,
    Arrow,
    Comma,
    Dot,
    Colon,
    Semicolon,
    Minus,
    MinusMinus,
    Plus,
    PlusPlus,
    Star,
    Percent,
    Slash,
    Equal,
    EqualEqual,
    Not,
    NotEqual,
    LessThan,
    LessThanEqual,
    GreaterThan,
    GreaterThanEqual,
}
