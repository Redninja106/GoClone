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
internal class FunctionExpression : IExpression
{
    public Function function;

    public LLVMValueRef Emit(EmitContext context, LLVMBuilderRef builder)
    {
        return function.llvmFunction;
    }

    public IType GetResultType()
    {
        var args = function.parameters.Select(p => p.type);
        if (function.receiver != null)
            args = args.Prepend(function.receiver.type);
        return new FunctionType { returnType = function.returnType, parameters = args.ToArray() };
    }

    public IExpression Resolve(IScope scope)
    {
        return this;
    }
}
