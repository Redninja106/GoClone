using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Expressions;
internal class GlobalVariableExpression : IExpression
{
    public VariableDeclaration declaration;

    public LLVMValueRef Emit(EmitContext context, LLVMBuilderRef builder)
    {
        return declaration.Value;
    }

    public IType GetResultType()
    {
        return declaration.variable.type;
    }

    public IExpression Resolve(IScope scope)
    {
        throw new NotImplementedException();
    }
}
