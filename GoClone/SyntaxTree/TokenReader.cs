using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree;
internal class TokenReader(Token[] tokens)
{
    Token[] tokens = tokens;
    int next = 0;

    public Token Current => tokens[next];

    public bool Peek(TokenKind kind, int offset = 1)
    {
        return Peek(offset).kind == kind;
    }

    public Token Peek(int offset = 1)
    {
        int index = next + offset;
        if (index < 0 || index >= tokens.Length)
        {
            return Token.EndOfSource;
        }
        return tokens[index];
    }

    public Token Next()
    {
        Token result = Current;
        next++;
        return result;
    }

    public bool Next(TokenKind kind)
    {
        if (Current.kind == kind)
        {
            Next();
            return true;
        }
        return false;
    }

    public bool ReadLineTerminator()
    {
        if (Next(TokenKind.Semicolon))
        {
            return true;
        }

        if (Peek(-1).hasLineTerminator)
        {
            return true;
        }
        return false;
    }

    public bool Next(TokenKind kind, out Token token)
    {
        if (Current.kind == kind)
        {
            token = Next();
            return true;
        }

        token = Token.Unknown;
        return false;
    }

    public Token NextOrError(TokenKind kind)
    {
        if (Next(kind))
        {
            return Peek(-1);
        }
        throw new Exception($"expected {kind} (instead, got '{Peek(0).Value}')!");
    }

    public TokenReader Clone()
    {
        TokenReader result = new(tokens);
        result.next = this.next;
        return result;
    }

    internal void ReadLineTerminatorOrError()
    {
        if (!ReadLineTerminator())
        {
            throw new Exception();
        }
    }

    public Token[] GetTokenArray()
    {
        return tokens;
    }
}
