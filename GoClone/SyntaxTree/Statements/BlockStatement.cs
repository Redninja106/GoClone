using GoClone.CodeGeneration;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Statements;
internal class BlockStatement : IStatement
{
    public List<IStatement> statements;

    public static BlockStatement Parse(TokenReader reader)
    {
        reader.NextOrError(TokenKind.OpenBrace);

        List<IStatement> statements = [];
        while (!reader.Next(TokenKind.CloseBrace))
        {
            statements.Add(Module.ParseStatement(reader));
        }

        return new BlockStatement { statements = statements };
    }

    public void Resolve(IScope scope)
    {
        BlockScope blockScope = new(scope, statements.OfType<LocalVariableStatement>());
        foreach (var statement in statements)
        {
            statement.Resolve(blockScope);
        }
    }

    public void Verify(IErrorHandler errorHandler)
    {
        foreach (var statement in statements)
        {
            statement.Verify(errorHandler);
        }
    }

    public void Emit(EmitContext context, LLVMBuilderRef builder)
    {
        foreach (var statement in statements)
        {
            statement.Emit(context, builder);
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new();

        sb.AppendLine("{");
        foreach (var statement in statements)
        {
            sb.Append("    ");
            sb.AppendLine(statement.ToString()!.Replace("\n", "\n    "));
        }

        sb.Append('}');

        return sb.ToString();
    }
}
