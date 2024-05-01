using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Expressions;
internal class TrueExpression : IExpression
{
    public LLVMValueRef Emit(EmitContext context, LLVMBuilderRef builder)
    {
        return LLVMValueRef.CreateConstInt(context.llvmCtx.Int1Type, 1);
    }

    public IType GetResultType()
    {
        return new PrimitiveType { PrimitiveKind = TokenKind.Bool };
    }

    public IExpression Resolve(IScope scope)
    {
        throw new NotImplementedException();
    }
}
