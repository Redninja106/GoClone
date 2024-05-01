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
            var value= builder.BuildExtractValue(reference, 0);
            // var value = builder.BuildLoad2(valuePtr.TypeOf, valuePtr);

            var vtablePtr = builder.BuildExtractValue(reference, 1);
            var vtable = builder.BuildLoad2(vtablePtr.TypeOf, vtablePtr);
            var fn = builder.BuildGEP2(vtablePtr.TypeOf, vtable, [LLVMValueRef.CreateConstInt(context.llvmCtx.Int32Type, ma.interfaceIdx)]);

            var fnType = LLVMTypeRef.CreateFunction(
                ma.interfaceFunction.returnType.Emit(context.llvmCtx),
                [
                    vtable.TypeOf, 
                    ..ma.interfaceFunction.parameters.Select(p => p.type.Emit(context.llvmCtx))
                ]
                );
            
            var args2 = arguments.Select(a => a.Emit(context, builder)).Prepend(value);

            return builder.BuildCall2(fnType, fn, args2.ToArray());
        }

        var calleeValue = callee.Emit(context, builder);

        IEnumerable<LLVMValueRef> args = arguments.Select(a => a.Emit(context, builder));

        if (callee is MemberAccessExpression ma2 && ma2.receiverTarget != null)
        {
            args = args.Prepend(ma2.value.Emit(context, builder));
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
