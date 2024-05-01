using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Statements;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Expressions;
internal class LocalVariableExpression : IExpression, IAssignable
{
    public LocalVariableStatement variable;

    public LLVMValueRef Emit(EmitContext context, LLVMBuilderRef builder)
    {
        return builder.BuildLoad2(variable.variable.type.Emit(context.llvmCtx), variable.Value);
    }

    public LLVMValueRef? EmitAssignablePointer(EmitContext context, LLVMBuilderRef builder)
    {
        return variable.Value;
    }

    public IType GetResultType()
    {
        return variable.variable.type;
    }

    public IExpression Resolve(IScope scope)
    {
        return this;
    }
}
