using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Expressions;
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
        variable.type = variable.type.Resolve(scope);
        variable.initializer = variable.initializer?.Resolve(scope);
    }

    public void Emit(EmitContext context, LLVMBuilderRef builder)
    {
        Value = builder.BuildAlloca(variable.type.Emit(context.llvmCtx));
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
