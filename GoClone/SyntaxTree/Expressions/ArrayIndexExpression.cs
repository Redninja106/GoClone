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
        var ptr = EmitAssignablePointer(context, builder)!.Value;

        return builder.BuildLoad2(this.GetResultType().Emit(context.llvmCtx), ptr);
    }

    public LLVMValueRef? EmitAssignablePointer(EmitContext context, LLVMBuilderRef builder)
    {
        IType memberType = array.GetResultType();
        IType arrayType = memberType;
        int indirectionCount = 0;
        while (arrayType.GetEffectiveType() is PointerType)
        {
            arrayType = (arrayType as PointerType)!.elementType;
            indirectionCount++;
        }

        if (arrayType is not ArrayType arr)
        {
            throw new();
        }

        if (indirectionCount > 0)
        {
            // we're indexing an array ptr ie int[]**
            // use GEP to find a ptr to the element

            LLVMValueRef elemPtr = array.Emit(context, builder);

            while (indirectionCount > 1)
            {
                elemPtr = builder.BuildLoad2(elemPtr.TypeOf, elemPtr);
                indirectionCount--;
            }

            var idx = index.Emit(context, builder);
            elemPtr = builder.BuildGEP2(arr.elementType.Emit(context.llvmCtx), elemPtr, [idx]);
            return elemPtr;

        }
        else if (index is NumberExpression number)
        {
            if (arr.length is null)
                throw new();
            var intNum = uint.Parse(number.number.ToString());
            return builder.BuildExtractValue(array.Emit(context, builder), (uint)intNum);
        }
        else
        {
            if (arr.length is null)
                throw new();

            throw new NotImplementedException();
        }


        //LLVMValueRef arrayPtr;
        //if (array is IAssignable assignable && assignable.EmitAssignablePointer(context, builder) is LLVMValueRef assignablePtr)
        //{
        //    var idx = index.Emit(context, builder);
        //    var ptr = builder.BuildGEP2(array.GetResultType().GetPointerElementType().Emit(context.llvmCtx), assignablePtr, [LLVMValueRef.CreateConstInt(context.llvmCtx.Int32Type, 0), idx]);
        //    if (array.GetResultType().GetEffectiveType().GetPointerElementType().GetEffectiveType() is ArrayType arr && arr.length is null)
        //    {
        //        ptr = builder.BuildLoad2(ptr.TypeOf, ptr);
        //    }
        //    return ptr;
        //}

        //return null;
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
