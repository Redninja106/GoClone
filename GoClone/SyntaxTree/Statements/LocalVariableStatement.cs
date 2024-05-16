using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Expressions;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Statements;
internal class LocalVariableStatement : IStatement, IResolvableValue
{
    public Variable variable;

    public LLVMValueRef Value { get; private set; }

    public static LocalVariableStatement Parse(TokenReader reader)
    {
        return new LocalVariableStatement { variable = Variable.Parse(reader) };
    }

    public void Resolve(IScope scope)
    {
        variable.type = variable.type?.Resolve(scope);
        variable.initializer = variable.initializer?.Resolve(scope);

        switch ((variable.type, variable.initializer))
        {
            case (null, null):
                throw new Exception("variable must declare a type or initializer");
            case (null, IExpression):
                variable.type = variable.initializer.GetResultType().Resolve(scope);
                break;
            case (IType, null):
                variable.initializer = new NullExpression() { type = variable.type }.Resolve(scope);
                break;
            case (IType, IExpression):
                if (!variable.type.Equals(variable.initializer.GetResultType()))
                {
                    variable.initializer = new CastExpression { type = variable.type, value = variable.initializer }.Resolve(scope);
                }
                break;
            default:
                throw new Exception();
        }
    }

    public void Verify(IErrorHandler errorHandler)
    {
    }

    public void Emit(EmitContext context, LLVMBuilderRef builder)
    {
        Value = builder.BuildAlloca(variable.type.Emit(context.llvmCtx), this.variable.name.ToString());
        if (variable.initializer != null)
        {
            var initializer = variable.initializer.Emit(context, builder);
            builder.BuildStore(initializer, Value);
        }
    }

    public override string ToString()
    {
        return variable.ToString();
    }
}
