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
internal class IdentifierExpression : IExpression
{
    public Token identifier;

    public IExpression Resolve(IScope scope)
    {
        var resolved = scope.ResolveValue(identifier);
        switch (resolved)
        {
            case VariableDeclaration var:
                return new GlobalVariableExpression { declaration = var };
            case LocalVariableStatement local:
                return new LocalVariableExpression { variable = local };
            case Function fn:
                return new FunctionExpression { function = fn };
            case Parameter p:
                return new ParameterExpression { parameter = p };
            default:
                throw new();
        }

        return this;
    }

    public LLVMValueRef Emit(EmitContext context, LLVMBuilderRef builder)
    {
        throw new Exception($"unresolved identifier {identifier}!");
    }

    public IType GetResultType()
    {
        // new UnknownType();
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return identifier.ToString();
    }
}
