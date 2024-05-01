using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Expressions;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree;
internal class VariableDeclaration : IDeclaration, IResolvableValue
{
    public Variable variable;

    private LLVMValueRef value;
    public LLVMValueRef Value => value;

    public static VariableDeclaration Parse(TokenReader reader)
    {
        return new VariableDeclaration { variable = Variable.Parse(reader) };
    }

    public void Emit(ModuleScope scope)
    {
        var global = scope.module.AddGlobal(this.variable.type.Emit(scope.context), this.variable.name.ToString());
        global.Initializer = LLVMValueRef.CreateConstInt(scope.context.Int32Type, 0);
    }


    public void Register(ModuleScope scope)
    {
        scope.valueDeclarations.Add(this.variable.name.ToString(), this);
    }

    public void Resolve(ModuleScope scope)
    {
        variable.type = variable.type.Resolve(scope);
        variable.initializer = variable.initializer.Resolve(scope);
    }

    public override string ToString()
    {
        return variable.ToString();
    }

    internal LLVMValueRef GetLLVMValue()
    {
        return value;
    }
}