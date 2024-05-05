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
internal class AddressOfExpression : IExpression
{
    public IExpression value;

    public IExpression Resolve(IScope scope)
    {
        value = value.Resolve(scope);
        return this;
    }

    public LLVMValueRef Emit(EmitContext context, LLVMBuilderRef builder)
    {
        switch (value)
        {
            case LocalVariableExpression var:
                return var.declStatement.Value;
            default:
                throw new($"Cannot take address of {value}");
        }

        throw new($"Cannot take address of {value}");
    }

    public IType GetResultType()
    {
        return new PointerType { elementType = value.GetResultType() };
    }

    public override string ToString()
    {
        return $"&{value}";
    }

}
