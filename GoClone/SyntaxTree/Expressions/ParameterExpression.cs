using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Expressions;
internal class ParameterExpression : IExpression, IAssignable
{
    public Parameter parameter;

    public LLVMValueRef Emit(EmitContext context, LLVMBuilderRef builder)
    {
        return parameter.value;
    }

    public LLVMValueRef? EmitAssignablePointer(EmitContext context, LLVMBuilderRef builder)
    {
        if (parameter.type is PointerType)
        {
            return parameter.value;
        }
        return null;
    }

    public IType GetResultType()
    {
        return parameter.type;
    }

    public IExpression Resolve(IScope scope)
    {
        return this;
    }

    public override string ToString()
    {
        return parameter.name.ToString();
    }
}
