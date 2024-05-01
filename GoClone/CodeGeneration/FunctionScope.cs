using GoClone.SyntaxTree;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.CodeGeneration;
internal class FunctionScope : IScope
{
    public Function function;

    private ModuleScope moduleScope;

    public FunctionScope(ModuleScope moduleScope, Function function)
    {
        this.moduleScope = moduleScope;
        this.function = function;
    }

    public FunctionScope GetFunction()
    {
        return this;
    }

    public ModuleScope GetModule()
    {
        return moduleScope;
    }

    public IResolvableValue ResolveValue(Token identifier)
    {
        if (function.receiver != null && function.receiver.name.Value.SequenceEqual(identifier.Value))
        {
            return function.receiver;
        }
        foreach (var parameter in function.parameters)
        {
            if (parameter.name.Value.SequenceEqual(identifier.Value))
            {
                return parameter;
            }
        }

        return moduleScope.ResolveValue(identifier);
    }

    public IResolvableType ResolveType(Token identifier)
    {
        return moduleScope.ResolveType(identifier);
    }

    public Function ResolveReceiver(IType type, Token name)
    {
        return moduleScope.ResolveReceiver(type, name);
    }

    public LLVMValueRef GetInterfaceVTable(IType type, IType iface)
    {
        return moduleScope.GetInterfaceVTable(type, iface);
    }
}
