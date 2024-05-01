using GoClone.CodeGeneration;
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
        return new FunctionType { returnType = function.returnType, parameters = function.parameters.Select(p => p.type).ToArray() };
    }

    public IExpression Resolve(IScope scope)
    {
        throw new NotImplementedException();
    }
}
