using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Expressions;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Statements;
internal class ReturnStatement : IStatement
{
    IExpression? returnValue;

    public static ReturnStatement Parse(TokenReader reader)
    {
        reader.NextOrError(TokenKind.Return);
        
        if (reader.ReadLineTerminator())
        {
            return new ReturnStatement { returnValue = null };
        }

        IExpression returnValue = Module.ParseExpression(reader);
        reader.ReadLineTerminatorOrError();
        return new ReturnStatement { returnValue = returnValue };
    }

    public void Resolve(IScope scope)
    {
        returnValue = returnValue?.Resolve(scope);
    }

    public void Emit(EmitContext context, LLVMBuilderRef builder)
    {
        if (this.returnValue == null)
        {
            builder.BuildRetVoid();
        }
        else
        {
            builder.BuildRet(returnValue.Emit(context, builder));
        }
    }

    public override string ToString()
    {
        StringBuilder result = new();

        result.Append("return");

        if (returnValue != null)
        {
            result.Append(' ');
            result.Append(returnValue);
        }

        return result.ToString();
    }
}
