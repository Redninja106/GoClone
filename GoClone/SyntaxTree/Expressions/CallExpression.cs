using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace GoClone.SyntaxTree.Expressions;
internal class CallExpression : IExpression
{
    public IExpression callee;
    public IExpression[] arguments;

    public IExpression Resolve(IScope scope)
    {
        callee = callee.Resolve(scope);
        for (int i = 0; i < arguments.Length; i++)
        {
            arguments[i] = arguments[i].Resolve(scope);
        }
        return this;
    }

    public LLVMValueRef Emit(EmitContext context, LLVMBuilderRef builder)
    {
        if (callee is MemberAccessExpression ma && ma.interfaceFunction != null)
        {
            var reference = ma.value.Emit(context, builder);
            var value = builder.BuildExtractValue(reference, 0);

            var vtablePtr = builder.BuildExtractValue(reference, 1);
            var fnPtrPtr = builder.BuildGEP2(vtablePtr.TypeOf, vtablePtr, [LLVMValueRef.CreateConstInt(context.llvmCtx.Int32Type, ma.interfaceIdx)]);
            var fnPtr = builder.BuildLoad2(vtablePtr.TypeOf, fnPtrPtr);

            var fnType = LLVMTypeRef.CreateFunction(
                ma.interfaceFunction.returnType?.Emit(context.llvmCtx) ?? context.llvmCtx.VoidType,
                [
                    fnPtr.TypeOf, 
                    ..ma.interfaceFunction.parameters.Select(p => p.type.Emit(context.llvmCtx))
                ]
                );
            
            var args2 = arguments.Select(a => a.Emit(context, builder)).Prepend(value);

            return builder.BuildCall2(fnType, fnPtr, args2.ToArray());
        }

        var calleeValue = callee.Emit(context, builder);

        IEnumerable<LLVMValueRef> args = arguments.Select(a => a.Emit(context, builder));

        if (callee is MemberAccessExpression ma2 && ma2.receiverTarget != null)
        {
            LLVMValueRef receiverValue;
            if (ma2.receiverTarget.receiver.type is PointerType)
            {
                if (ma2.value is IAssignable assignable && assignable.EmitAssignablePointer(context, builder) is LLVMValueRef assignablePtr)
                {
                    receiverValue = assignablePtr;
                }
                else
                {
                    throw new("this receiver may only be called on an addressable value!");
                }
            }
            else
            {
                receiverValue = ma2.value.Emit(context, builder);
            }

            args = args.Prepend(receiverValue);
        }

        LLVMValueRef[] values = args.ToArray();

        return builder.BuildCall2(callee.GetResultType().Emit(context.llvmCtx), calleeValue, values);
    }

    public IType GetResultType()
    {
        return ((FunctionType)callee.GetResultType()).returnType;
    }

    public override string ToString()
    {
        StringBuilder sb = new();

        sb.Append(callee.ToString());
        sb.Append('(');
        foreach (var arg in arguments)
        {
            sb.Append(arg);
            if (arg != arguments[^1])
                sb.Append(", ");
        }
        sb.Append(')');

        return sb.ToString();
    }

}
