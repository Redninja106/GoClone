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
    public LocalVariableStatement declStatement;

    public LLVMValueRef Emit(EmitContext context, LLVMBuilderRef builder)
    {
        return builder.BuildLoad2(declStatement.variable.type.Emit(context.llvmCtx), declStatement.Value);
    }

    public LLVMValueRef? EmitAssignablePointer(EmitContext context, LLVMBuilderRef builder)
    {
        return declStatement.Value;
    }

    public void Verify(IErrorHandler errorHandler)
    {
    }

    public IType GetResultType()
    {
        return declStatement.variable.type;
    }

    public IExpression Resolve(IScope scope)
    {
        return this;
    }

    public override string ToString()
    {
        return this.declStatement.variable.name.ToString();
    }
}
