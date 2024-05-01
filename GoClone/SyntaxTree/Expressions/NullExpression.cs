using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Expressions;
internal class NullExpression : IExpression
{
    public IType type;

    public unsafe LLVMValueRef Emit(EmitContext context, LLVMBuilderRef builder)
    {
        return new((nint)LLVM.ConstNull((LLVMOpaqueType*)type.Emit(context.llvmCtx)));
    }

    public IType GetResultType()
    {
        return type;
    }

    public IExpression Resolve(IScope scope)
    {
        type = type.Resolve(scope);
        return this;
    }

    public override string ToString()
    {
        return $"null({type})";
    }
}
