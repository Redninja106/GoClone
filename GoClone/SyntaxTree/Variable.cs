using GoClone.SyntaxTree.Expressions;
using GoClone.SyntaxTree.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree;
internal class Variable
{
    public IType? type;
    public Token name;
    public IExpression? initializer;

    public static Variable Parse(TokenReader reader)
    {
        reader.NextOrError(TokenKind.Var);
        var name = reader.NextOrError(TokenKind.Identifier);

        IType? type = null;
        if (reader.Next(TokenKind.Arrow))
        {
            type = Module.ParseType(reader);
        }

        IExpression? initializer = null;
        if (reader.Next(TokenKind.Equal))
        {
            initializer = Module.ParseExpression(reader);
        }

        return new Variable { type = type, name = name, initializer = initializer };
    }

    public override string ToString()
    {
        StringBuilder sb = new();

        sb.Append("var ");
        sb.Append(name);

        if (type != null)
        {
            sb.Append(" -> ");
            sb.Append(type);
        }

        if (initializer != null)
        {
            sb.Append(" = ");
            sb.Append(initializer);
        }

        return sb.ToString();
    }
}
