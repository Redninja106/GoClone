using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Expressions;
internal class MemberAccessExpression : IExpression, IAssignable
{
    public IExpression value;
    public Token identifier;

    public uint fieldIdx;
    public Parameter? field;

    public Function? receiverTarget;

    public Function? interfaceFunction;
    public uint interfaceIdx;

    public IExpression Resolve(IScope scope)
    {
        value = value.Resolve(scope);
        var valueType = value.GetResultType();

        if (valueType.GetBaseType() is StructType structType)
        {
            for (int i = 0; i < structType.fields.Count; i++)
            {
                if (structType.fields[i].name.Value.SequenceEqual(identifier.Value))
                {
                    field = structType.fields[i];
                    fieldIdx = (uint)i;
                }
            }
        } 
        else if (valueType.GetBaseType() is InterfaceType interfaceType)
        {
            for (int i = 0; i < interfaceType.functions.Count; i++)
            {
                if (interfaceType.functions[i].name.Value.SequenceEqual(identifier.Value))
                {
                    interfaceFunction = interfaceType.functions[i];
                    interfaceIdx = (uint)i;
                }
            }
        }

        // receiver function
        if (field is null && interfaceFunction is null)
        {
            receiverTarget = scope.ResolveReceiver(valueType.GetPointerElementType(), identifier);
        }

        return this;
    }


    public LLVMValueRef Emit(EmitContext context, LLVMBuilderRef builder)
    {
        if (field is not null)
        {
            var structType = value.GetResultType().GetPointerElementType().Emit(context.llvmCtx);
            LLVMValueRef ptr;
            if (value is IAssignable assignable && assignable.EmitAssignablePointer(context, builder) is LLVMValueRef assignablePtr)
            {
                ptr = assignablePtr;
                var resultPtr = builder.BuildStructGEP2(structType, ptr, fieldIdx);
                return builder.BuildLoad2(field.type.Emit(context.llvmCtx), resultPtr);
            }
            else
            {
                var v = value.Emit(context, builder);
                return builder.BuildExtractValue(v, fieldIdx);
            }
        }
        else if (interfaceFunction is not null)
        {
            return interfaceFunction.llvmFunction;
        }
        else if (receiverTarget is not null)
        {
            return receiverTarget.llvmFunction;
        }
        else
        {
            throw new();
        }
    }

    public IType GetResultType()
    {
        if (field is not null)
        {
            return field.type;
        }
        if (interfaceFunction is not null)
        {
            return interfaceFunction.GetFunctionType();
        }
        if (receiverTarget is not null)
        {
            return receiverTarget.GetFunctionType();
        }
        throw new();
    }

    public override string ToString()
    {
        return $"{value}.{identifier}";
    }

    public LLVMValueRef? EmitAssignablePointer(EmitContext context, LLVMBuilderRef builder)
    {
        if (value is IAssignable assignable)
        {
            var structType = value.GetResultType().Emit(context.llvmCtx);
            var ptr = assignable.EmitAssignablePointer(context, builder);
            if (ptr != null)
            {
                return builder.BuildStructGEP2(structType, ptr.Value, fieldIdx);
            }
        }

        return null;
    }
}
