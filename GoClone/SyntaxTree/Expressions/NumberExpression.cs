using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Expressions;
internal class NumberExpression : IExpression
{
    public Token number;

    public IExpression Resolve(IScope scope)
    {
        return this;
    }

    public LLVMValueRef Emit(EmitContext context, LLVMBuilderRef builder)
    {
        return LLVMValueRef.CreateConstInt(context.llvmCtx.Int32Type, (ulong)int.Parse(number.ToString()), false);
    }

    public IType GetResultType()
    {
        return new PrimitiveType { PrimitiveKind = TokenKind.Int };
    }

    public override string ToString()
    {
        return number.ToString();
    }
}
