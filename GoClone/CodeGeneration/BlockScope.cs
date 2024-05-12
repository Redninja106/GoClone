using GoClone.SyntaxTree;
using GoClone.SyntaxTree.Statements;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.CodeGeneration;
internal class BlockScope : IScope
{
    IScope parentScope;
    Dictionary<string, LocalVariableStatement> declarations = [];

    public BlockScope(IScope parentScope, IEnumerable<LocalVariableStatement> statements)
    {
        this.parentScope = parentScope;

        foreach (var stmt in statements)
        {
            this.declarations.Add(stmt.variable.name.ToString(), stmt);
        }
    }

    public FunctionScope GetFunction()
    {
        return parentScope.GetFunction();
    }

    public ModuleScope GetModule()
    {
        return parentScope.GetModule();
    }

    public IResolvableValue ResolveValue(Token identifier)
    {
        if (declarations.TryGetValue(identifier.ToString(), out LocalVariableStatement? value))
        {
            return value;
        }

        return parentScope.ResolveValue(identifier);
    }

    public IResolvableType ResolveType(Token identifier)
    {
        return parentScope.ResolveType(identifier);
    }

    public Function ResolveReceiver(IType type, Token name)
    {
        return parentScope.ResolveReceiver(type, name);
    }

    public Function ResolveOperator(IType type, OverloadableOperator op)
    {
        return parentScope.ResolveOperator(type, op);
    }


    public LLVMValueRef GetInterfaceVTable(IType type, IType iface)
    {
        return parentScope.GetInterfaceVTable(type, iface);
    }
}
