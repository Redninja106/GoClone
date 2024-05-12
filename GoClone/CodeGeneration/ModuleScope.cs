using GoClone.SyntaxTree;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.CodeGeneration;
internal class ModuleScope(LLVMContextRef context, LLVMModuleRef module, IErrorHandler errorHandler) : IScope
{
    public Dictionary<string, IResolvableValue> valueDeclarations = [];
    public Dictionary<string, IResolvableType> typeDeclarations = [];
    public Dictionary<IType, Dictionary<string, Function>> receivers = new(EqualityComparer<IType>.Default);
    public Dictionary<IType, Dictionary<OverloadableOperator, Function>> operators = new(EqualityComparer<IType>.Default);
    public Dictionary<IType, Dictionary<InterfaceType, LLVMValueRef>> vtables = new(EqualityComparer<IType>.Default);
    public List<ModuleScope> importedScopes = [];

    public LLVMContextRef context = context;
    public LLVMModuleRef module = module;
    public IErrorHandler errorHandler = errorHandler;

    public FunctionScope GetFunction()
    {
        throw new InvalidOperationException();
    }

    public ModuleScope GetModule()
    {
        return this;
    }

    public IResolvableValue ResolveValue(Token identifier)
    {
        if (valueDeclarations.TryGetValue(identifier.ToString(), out var value))
        {
            return value;
        }

        foreach (var imported in importedScopes)
        {
            if (imported.valueDeclarations.TryGetValue(identifier.ToString(), out value))
            {
                return value;
            }
        }

        throw new Exception($"unknown identifier {identifier}");
    }

    public IResolvableType ResolveType(Token identifier)
    {
        if (typeDeclarations.TryGetValue(identifier.ToString(), out var value))
        {
            return value;
        }

        foreach (var imported in importedScopes)
        {
            if (imported.typeDeclarations.TryGetValue(identifier.ToString(), out value))
            {
                return value;
            }
        }

        throw new Exception($"unknown type {identifier}");
    }

    public Function ResolveOperator(IType type, OverloadableOperator op)
    {
        type = type.GetElementType();

        if (operators.TryGetValue(type, out var operatorMap) && operatorMap.TryGetValue(op, out var fn))
        {
            return fn;
        }

        foreach (var imported in importedScopes)
        {
            if (imported.operators.TryGetValue(type, out operatorMap) && operatorMap.TryGetValue(op, out fn))
            {
                return fn;
            }
        }

        throw new Exception($"type {type} has no {op} overload!");
    }

    public Function ResolveReceiver(IType type, Token name)
    {
        if (receivers.TryGetValue(type, out var receiverMap) && receiverMap.TryGetValue(name.ToString(), out var fn))
        {
            return fn;
        }

        foreach (var imported in importedScopes)
        {
            if (imported.receivers.TryGetValue(type, out receiverMap) && receiverMap.TryGetValue(name.ToString(), out fn))
            {
                return fn;
            }
        }

        throw new Exception($"type {type} has no receiver {name}");
    }

    public LLVMValueRef GetInterfaceVTable(IType type, IType iface)
    {
        if (!vtables.TryGetValue(type, out var imap))
        {
            vtables.Add(type, imap = []);
        }
        var interfaceType = (InterfaceType)iface.GetBaseType();

        if (imap.TryGetValue(interfaceType, out var vtable))
        {
            return vtable;
        }
        else
        {
            List<Function> functions = [];
            foreach (var func in interfaceType.functions)
            {
                if (func.op != null)
                {
                    functions.Add(ResolveOperator(type.GetElementType(), func.op!.Value));
                }
                else
                {
                    functions.Add(ResolveReceiver(type.GetElementType(), func.name));
                }
            }
            unsafe
            {
                var vtableGlobal = module.AddGlobal(LLVMTypeRef.CreateArray(LLVM.PointerTypeInContext(context, 0), (uint)functions.Count), $"{type}_vtbl_for_{iface}");
                imap.Add(interfaceType, vtableGlobal);
                return vtableGlobal;
            }
        }
    }

    internal unsafe void BuildVTables()
    {
        foreach (var (type, imap) in vtables)
        {
            foreach (var (iface, global) in imap)
            {
                List<Function> functions = [];
                foreach (var func in iface.functions)
                {
                    if (func.op != null)
                    {
                        functions.Add(ResolveOperator(type.GetElementType(), func.op.Value));
                    }
                    else
                    {
                        functions.Add(ResolveReceiver(type.GetElementType(), func.name));
                    }
                }

                var opaquePtrType = LLVM.PointerTypeInContext(context, 0);
                var vtable = LLVMValueRef.CreateConstArray(opaquePtrType, functions.Select(f => LLVMValueRef.CreateConstBitCast(f.interfaceStub ?? f.llvmFunction, opaquePtrType)).ToArray());
                
                unsafe
                {
                    LLVM.SetInitializer(global, vtable);
                }
            }
        }
    }
}