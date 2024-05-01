using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Expressions;
internal class StructLiteralExpression : IExpression
{
    public IType? type;
    public IExpression[] values;

    public unsafe LLVMValueRef Emit(EmitContext context, LLVMBuilderRef builder)
    {
        LLVMValueRef value = LLVM.ConstNull(type.Emit(context.llvmCtx));
        for (int i = 0; i < values.Length; i++)
        {
            value = builder.BuildInsertValue(value, values[i].Emit(context, builder), (uint)i);
        }
        return value;
    }

    public IType GetResultType()
    {
        return type;
    }

    public IExpression Resolve(IScope scope)
    {
        type = type?.Resolve(scope);
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = values[i].Resolve(scope);
        }
        return this;
    }
}
