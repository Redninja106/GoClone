using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Expressions;
internal class FalseExpression : IExpression
{
    public IExpression Resolve(IScope scope)
    {
        return this;
    }

    public LLVMValueRef Emit(EmitContext context, LLVMBuilderRef builder)
    {
        return LLVMValueRef.CreateConstInt(context.llvmCtx.Int1Type, 0);
    }

    public IType GetResultType()
    {
        return new PrimitiveType { PrimitiveKind = TokenKind.Bool };
    }
}
