using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Expressions;
internal class DereferenceExpression : IExpression, IAssignable
{
    public IExpression value;

    public IExpression Resolve(IScope scope)
    {
        value = value.Resolve(scope);
        return this;
    }

    public LLVMValueRef Emit(EmitContext context, LLVMBuilderRef builder)
    {
        var val = value.Emit(context, builder);
        return builder.BuildLoad2((value.GetResultType() as PointerType).elementType.Emit(context.llvmCtx), val);
    }

    public IType GetResultType()
    {
        return (value.GetResultType().GetEffectiveType() as PointerType).elementType;
    }

    public LLVMValueRef? EmitAssignablePointer(EmitContext context, LLVMBuilderRef builder)
    {
        return value.Emit(context, builder);
    }
}
