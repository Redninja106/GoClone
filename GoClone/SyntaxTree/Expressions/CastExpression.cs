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
        if (type.GetEffectiveType() is InterfaceType || type.GetEffectiveType() is ReferenceType refType && refType.elementType.GetEffectiveType() is InterfaceType)
        {
            return BuildMakeRef(context, builder, interfaceVtbl);
            // LLVMValueRef valuePtr = GetValuePtr(context, builder);
            // unsafe 
            // {
            //     LLVMTypeRef opaquePtrType = LLVM.PointerTypeInContext((LLVMOpaqueContext*)context.llvmCtx, 0);
            //     LLVMValueRef opaqueNullPtr = LLVM.ConstNull(opaquePtrType);
            //     
            //     var nullRef = context.llvmCtx.GetConstStruct([opaqueNullPtr, opaqueNullPtr], false);
            //     var withValuePtr = builder.BuildInsertValue(nullRef, builder.BuildBitCast(valuePtr, opaquePtrType), 0);
            //     var withVtbl = builder.BuildInsertValue(withValuePtr, builder.BuildBitCast(interfaceVtbl, opaquePtrType), 1);
            // 
            //     return withVtbl;
            // }
        }

        if (value is StructLiteralExpression literal)
        {
            literal.type = type;
            return value.Emit(context, builder);
        }

        if (value.GetResultType()!.GetEffectiveType() is PointerType ptrTo && type.GetEffectiveType() is PointerType ptrFrom)
        {
            return builder.BuildBitCast(value.Emit(context, builder), value.GetResultType()!.Emit(context.llvmCtx));
        }

        if (value.GetResultType().GetEffectiveType() is ArrayType array && 
            type.GetEffectiveType() is ReferenceType reference && 
            reference.elementType is ArrayType targetArray && 
            targetArray.length == null)
        {
            return BuildMakeRef(context, builder, LLVMValueRef.CreateConstInt(context.llvmCtx.Int64Type, (ulong)array.intLength));
        }

        if (value.GetResultType().GetEffectiveType() is ReferenceType referenceType && type is StructType s && s.fields.Count == 2 && s.fields.All(f => f.type is PointerType))
        {
            return value.Emit(context, builder);
            return builder.BuildBitCast(value.Emit(context, builder), type.Emit(context.llvmCtx));
        }
        
        if (value.GetResultType().GetEffectiveType() is PointerType ptr && type is PrimitiveType intTo)
        {
            return builder.BuildPtrToInt(value.Emit(context, builder), intTo.Emit(context.llvmCtx));
        }

        if (value.GetResultType().GetEffectiveType() is PrimitiveType intFrom && type is PointerType ptr2)
        {
            return builder.BuildIntToPtr(value.Emit(context, builder), ptr2.Emit(context.llvmCtx));
        }

        throw new NotImplementedException();
    }

    private LLVMValueRef BuildMakeRef(EmitContext context, LLVMBuilderRef builder, LLVMValueRef vtbl)
    {
        LLVMValueRef valuePtr = GetValuePtr(context, builder);

        unsafe
        {
            LLVMTypeRef opaquePtrType = LLVM.PointerTypeInContext((LLVMOpaqueContext*)context.llvmCtx, 0);
            LLVMValueRef opaqueNullPtr = LLVM.ConstNull(opaquePtrType);

            if (vtbl.IsAConstantInt.Handle != 0)
            {
                vtbl = LLVMValueRef.CreateConstIntToPtr(vtbl, opaquePtrType);
            }

            var nullRef = context.llvmCtx.GetConstStruct([opaqueNullPtr, opaqueNullPtr], false);
            var withValuePtr = builder.BuildInsertValue(nullRef, builder.BuildBitCast(valuePtr, opaquePtrType), 0);
            var withVtbl = builder.BuildInsertValue(withValuePtr, builder.BuildBitCast(vtbl, opaquePtrType), 1);

            return withVtbl;
        }
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
