using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Expressions;
using GoClone.SyntaxTree.Statements;
using GoClone.SyntaxTree.Types;

namespace GoClone.SyntaxTree;
internal class Module
{
    public List<IDeclaration> declarations;

    public static Module Parse(TokenReader reader)
    {
        List<IDeclaration> declarations = [];
        while (!reader.Next(TokenKind.EndOfSource))
        {
            declarations.Add(ParseDeclaration(reader));
        }
        return new Module { declarations = declarations };
    }

    public static IType ParseType(TokenReader reader)
    {
        IType elementType = ParseElementType(reader);

        while (true) 
        {
            if (reader.Next(TokenKind.OpenBracket))
            {
                Token? length = null;
                if (reader.Next(TokenKind.Number, out Token n))
                {
                    length = n;
                }

                reader.NextOrError(TokenKind.CloseBracket);
                elementType = new ArrayType { elementType = elementType, length = length };
                continue;
            }
            if (reader.Next(TokenKind.Star))
            {
                elementType = new PointerType { elementType = elementType };
                continue;
            }
            if (reader.Next(TokenKind.And))
            {
                elementType = new ReferenceType { elementType = elementType };
                continue;
            }
            break;
        }

        return elementType;
    }

    public static IType ParseElementType(TokenReader reader)
    {
        if (reader.Next(TokenKind.Int))
        {
            return new PrimitiveType() { PrimitiveKind = TokenKind.Int };
        }

        if (reader.Next(TokenKind.Bool))
        {
            return new PrimitiveType() { PrimitiveKind = TokenKind.Bool };
        }

        if (reader.Next(TokenKind.Identifier, out Token identifier))
        {
            return new IdentifierType() { Identifier = identifier };
        }

        if (reader.Current.kind == (TokenKind.Struct))
        {
            return StructType.Parse(reader);
        }

        if (reader.Current.kind == (TokenKind.Interface))
        {
            return InterfaceType.Parse(reader);
        }

        throw new Exception();
    }

    public static IDeclaration ParseDeclaration(TokenReader reader)
    {
        if (reader.Current.kind == TokenKind.Func)
        {
            return Function.Parse(reader);
        }
        if (reader.Current.kind == TokenKind.Type)
        {
            return TypeDeclaration.Parse(reader);
        }
        if (reader.Current.kind == TokenKind.Var)
        {
            return VariableDeclaration.Parse(reader);
        }
        if (reader.Current.kind == TokenKind.Module)
        {
            return ModuleDeclaration.Parse(reader);
        }

        throw new();
    }

    public override string ToString()
    {
        StringBuilder sb = new();

        foreach (var decl in declarations)
        {
            sb.AppendLine(decl.ToString());
        }

        return sb.ToString();
    }

    public static IStatement ParseStatement(TokenReader reader)
    {
        if (reader.Current.kind == TokenKind.OpenBrace)
        {
            return BlockStatement.Parse(reader);
        }
        if (reader.Current.kind == TokenKind.If)
        {
            return ConditionalStatement.Parse(reader);
        }
        if (reader.Current.kind == TokenKind.Var)
        {
            return LocalVariableStatement.Parse(reader);
        }
        if (reader.Current.kind is TokenKind.Identifier or TokenKind.Number or TokenKind.OpenParenthesis or TokenKind.Star)
        {
            return ExpressionStatement.Parse(reader);
        }
        if (reader.Current.kind == TokenKind.Return)
        {
            return ReturnStatement.Parse(reader);
        }
        if (reader.Current.kind == TokenKind.While)
        {
            return WhileStatement.Parse(reader);
        }

        throw new();
    }

    public static IExpression ParseExpression(TokenReader reader)
    {
        return ParseWithPrecedence(reader, Precedence.None);
    }

    public static IExpression ParseWithPrecedence(TokenReader reader, Precedence precedence)
    {
        return ParseRestWithPrecedence(reader, ParseTerm(reader), precedence);
    }

    private static IExpression ParseRestWithPrecedence(TokenReader reader, IExpression left, Precedence precedence)
    {
        while (true)
        {
            if (reader.ReadLineTerminator())
            {
                return left;
            }

            TokenKind op = reader.Current.kind;
            Precedence opPrecedence = GetOperatorPrecedence(op);
            if (opPrecedence is Precedence.None)
            {
                break;
            }

            bool isLeftAssociative = precedence != Precedence.Assignment;
            if (opPrecedence < precedence || (isLeftAssociative && opPrecedence == precedence))
            {
                break;
            }

            // consume the operator
            reader.Next();

            var right = ParseWithPrecedence(reader, opPrecedence);
            left = new BinaryExpression() { op = op, left = left, right = right };
        }

        return left;
    }

    private static IExpression ParseTerm(TokenReader reader)
    {
        // parse prefix ops recursively
        if (reader.Next(TokenKind.Star))
        {
            return new DereferenceExpression() { value = ParseTerm(reader) };
        }
        if (reader.Next(TokenKind.And))
        {
            return new AddressOfExpression() { value = ParseTerm(reader) };
        }

        var term = ParseTermOnly(reader);
        
        // parse any number of postfixe ops
        List<IExpression> arguments = [];
        while (true)
        {
            if (reader.Next(TokenKind.OpenParenthesis))
            {
                if (!reader.Next(TokenKind.CloseParenthesis))
                {
                    arguments.Add(ParseExpression(reader));
                    while (!reader.Next(TokenKind.CloseParenthesis))
                    {
                        reader.NextOrError(TokenKind.Comma);
                        arguments.Add(ParseExpression(reader));
                    }
                }

                term = new CallExpression { callee = term, arguments = arguments.ToArray() };
                continue;
            }
            else if (reader.Next(TokenKind.Dot))
            {
                var identifier = reader.NextOrError(TokenKind.Identifier);
                term = new MemberAccessExpression { value = term, identifier = identifier };
                continue;
            }
            else if (reader.Next(TokenKind.As))
            {
                var type = ParseType(reader);
                term = new CastExpression { value = term, type = type };
            }
            else
            {
                break;
            }
        }

        return term;
    }

    private static IExpression ParseTermOnly(TokenReader reader)
    {
        // terms
        if (reader.Next(TokenKind.OpenParenthesis))
        {
            var result = ParseWithPrecedence(reader, Precedence.None);
            reader.NextOrError(TokenKind.CloseParenthesis);
            return result;
        }
        if (reader.Next(TokenKind.Number, out Token number))
        {
            return new NumberExpression { number = number };
        }
        if (reader.Next(TokenKind.Identifier, out Token identifier))
        {
            return new IdentifierExpression { identifier = identifier };
        }
        if (reader.Next(TokenKind.Character, out Token character))
        {
            return new CharacterExpression { character = character };
        }
        if (reader.Next(TokenKind.True))
        {
            return new TrueExpression();
        }
        if (reader.Next(TokenKind.False))
        {
            return new FalseExpression();
        }
        if (reader.Next(TokenKind.Null))
        {
            reader.NextOrError(TokenKind.OpenParenthesis);
            var type = ParseType(reader);
            reader.NextOrError(TokenKind.CloseParenthesis);
            return new NullExpression { type = type };
        }
        if (reader.Next(TokenKind.OpenBrace))
        {
            List<IExpression> values = [];
            if (!reader.Next(TokenKind.CloseBrace))
            {
                values.Add(ParseExpression(reader));
                while (!reader.Next(TokenKind.CloseBrace))
                {
                    reader.NextOrError(TokenKind.Comma);
                    values.Add(ParseExpression(reader));
                }
            }

            return new StructLiteralExpression() { type = null, values = values.ToArray() };
        }
        throw new Exception($"unknown token {reader.Current.kind} ({reader.Current})");
    }

    public static Precedence GetOperatorPrecedence(TokenKind op)
    {
        switch (op)
        {
            case TokenKind.Minus:
            case TokenKind.Plus:
                return Precedence.Additive;
            case TokenKind.Star:
            case TokenKind.Slash:
            case TokenKind.Percent:
                return Precedence.Multiplicative;
            case TokenKind.Equal:
                return Precedence.Assignment;
            case TokenKind.EqualEqual:
            case TokenKind.NotEqual:
            case TokenKind.LessThan:
            case TokenKind.LessThanEqual:
            case TokenKind.GreaterThan:
            case TokenKind.GreaterThanEqual:
                return Precedence.Equality;
            default:
                return Precedence.None;
        }
    }
}

enum Precedence
{
    None,
    Assignment,
    LogicalOr,
    LogicalAnd,
    BitwiseOr,
    BitwiseXOr,
    BitwiseAnd,
    Equality,
    Relational,
    BitShift,
    Additive,
    Multiplicative,
    UnaryPrefix,
    UnaryPostfix
}