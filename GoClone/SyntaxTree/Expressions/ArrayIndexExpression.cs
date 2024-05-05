using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Expressions;
internal class ArrayIndexExpression : IExpression, IAssignable
{
    public IExpression array;
    public IExpression index;

    public LLVMValueRef Emit(EmitContext context, LLVMBuilderRef builder)
    {
        LLVMValueRef arrayPtr;
        if (array is IAssignable assignable && assignable.EmitAssignablePointer(context, builder) is LLVMValueRef assignablePtr)
        {
            arrayPtr = assignablePtr;
        }
        else
        {
            Console.WriteLine("WARNING: ARRAY COPY");
            arrayPtr = builder.BuildAlloca(array.GetResultType().Emit(context.llvmCtx));
            builder.BuildStore(array.Emit(context, builder), arrayPtr);
        }

        var idx = index.Emit(context, builder);
        LLVMValueRef ptr;
        if (array.GetResultType().GetEffectiveType().GetPointerElementType().GetEffectiveType() is ArrayType arr && arr.length is null)
        {
            ptr = builder.BuildLoad2(arrayPtr.TypeOf, arrayPtr);
            ptr = builder.BuildGEP2(arr.elementType.Emit(context.llvmCtx), ptr, [idx]);
        }
        else
        {
            ptr = builder.BuildGEP2(array.GetResultType().GetPointerElementType().Emit(context.llvmCtx), arrayPtr, [LLVMValueRef.CreateConstInt(context.llvmCtx.Int32Type, 0), idx]);
        }
        return builder.BuildLoad2(this.GetResultType().Emit(context.llvmCtx), ptr);
    }

    public LLVMValueRef? EmitAssignablePointer(EmitContext context, LLVMBuilderRef builder)
    {
        LLVMValueRef arrayPtr;
        if (array is IAssignable assignable && assignable.EmitAssignablePointer(context, builder) is LLVMValueRef assignablePtr)
        {
            var idx = index.Emit(context, builder);
            return builder.BuildGEP2(array.GetResultType().GetPointerElementType().Emit(context.llvmCtx), assignablePtr, [LLVMValueRef.CreateConstInt(context.llvmCtx.Int32Type, 0), idx]);
        }
        return null;
    }

    public IType? GetResultType()
    {   
        return (array.GetResultType().GetPointerElementType() as ArrayType).elementType;
    }

    public IExpression Resolve(IScope scope)
    {
        array = array.Resolve(scope);
        index = index.Resolve(scope);
        return this;
    }
}
