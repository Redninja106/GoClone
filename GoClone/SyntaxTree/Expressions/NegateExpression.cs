using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Expressions;
internal class NegateExpression : IExpression
{
    public IExpression value;

    public LLVMValueRef Emit(EmitContext context, LLVMBuilderRef builder)
    {
        return builder.BuildNeg(value.Emit(context, builder));
    }

    public IType? GetResultType()
    {
        return value.GetResultType();
    }

    public IExpression Resolve(IScope scope)
    {
        value = value.Resolve(scope);
        return this;
    }
}
