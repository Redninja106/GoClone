using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Expressions;
internal class SizeOfExpression : IExpression
{
    public IType type;

    public unsafe LLVMValueRef Emit(EmitContext context, LLVMBuilderRef builder)
    {
        return builder.BuildIntCast(new((nint)LLVM.SizeOf((LLVMOpaqueType*)type.Emit(context.llvmCtx))), context.llvmCtx.Int32Type);
    }

    public IType GetResultType()
    {
        return new PrimitiveType { PrimitiveKind = TokenKind.Int };
    }

    public IExpression Resolve(IScope scope)
    {
        type = type.Resolve(scope);
        return this;
    }

    public override string ToString()
    {
        return $"sizeof({type})";
    }
}
