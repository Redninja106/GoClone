using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Expressions;

internal class CastExpression : IExpression
{
    public IExpression value;
    public IType type;

    private LLVMValueRef interfaceVtbl;

    public LLVMValueRef Emit(EmitContext context, LLVMBuilderRef builder)
    {
        if (type.GetBaseType() is InterfaceType interfaceType)
        {
            LLVMValueRef valuePtr = GetValuePtr(context, builder);
            unsafe 
            {
                LLVMTypeRef opaquePtrType = LLVM.PointerTypeInContext((LLVMOpaqueContext*)context.llvmCtx, 0);
                LLVMValueRef opaqueNullPtr = LLVM.ConstNull(opaquePtrType);
                
                var nullRef = context.llvmCtx.GetConstStruct([opaqueNullPtr, opaqueNullPtr], false);
                var withValuePtr = builder.BuildInsertValue(nullRef, builder.BuildBitCast(valuePtr, opaquePtrType), 0);
                var withVtbl = builder.BuildInsertValue(withValuePtr, builder.BuildBitCast(interfaceVtbl, opaquePtrType), 1);

                return withVtbl;
            }
        }

        if (value is StructLiteralExpression literal)
        {
            literal.type = type;
            return value.Emit(context, builder);
        }

        throw new NotImplementedException();
    }

    private LLVMValueRef GetValuePtr(EmitContext context, LLVMBuilderRef builder)
    {
        if (value is IAssignable assignable)
        {
            var ptr = assignable.EmitAssignablePointer(context, builder);
            if (ptr != null)
            {
                return ptr.Value;
            }
        }

        var varPtr = builder.BuildAlloca(value.GetResultType().Emit(context.llvmCtx));
        builder.BuildStore(value.Emit(context, builder), varPtr);
        return varPtr;
    }

    public IType GetResultType()
    {
        return type;
    }

    public IExpression Resolve(IScope scope)
    {
        value = value.Resolve(scope);
        type = type.Resolve(scope);
        
        if (type.GetBaseType() is InterfaceType interfaceType)
        {
            interfaceVtbl = scope.GetInterfaceVTable(value.GetResultType(), type);
        }

        return this;
    }

    public override string ToString()
    {
        return $"{this.value} as {this.type}";
    }
}
