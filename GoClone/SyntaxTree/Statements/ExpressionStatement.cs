using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Expressions;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Statements;
internal class ExpressionStatement : IStatement
{
    public IExpression expression;

    public static ExpressionStatement Parse(TokenReader reader)
    {
        IExpression expression = Module.ParseExpression(reader);
        reader.ReadLineTerminatorOrError();
        return new ExpressionStatement { expression = expression };
    }

    public void Resolve(IScope scope)
    {
        expression.Resolve(scope);
    }

    public void Emit(EmitContext context, LLVMBuilderRef builder)
    {
        expression.Emit(context, builder);
    }

    public override string ToString()
    {
        return expression.ToString()!;
    }

    public void Verify(IErrorHandler errorHandler)
    {
    }
}
