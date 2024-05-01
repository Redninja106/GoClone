using GoClone.SyntaxTree;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.CodeGeneration;
internal class ModuleScope(LLVMContextRef context, LLVMModuleRef module) : IScope
{
    public Dictionary<string, IResolvableValue> valueDeclarations = [];
    public Dictionary<string, IResolvableType> typeDeclarations = [];
    public Dictionary<IType, Dictionary<string, Function>> receivers = new(EqualityComparer<IType>.Default);
    public Dictionary<IType, Dictionary<InterfaceType, LLVMValueRef>> vtables = new(EqualityComparer<IType>.Default);

    public LLVMContextRef context = context;
    public LLVMModuleRef module = module;

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
        return valueDeclarations[identifier.ToString()];
    }

    public IResolvableType ResolveType(Token identifier)
    {
        return typeDeclarations[identifier.ToString()];
    }

    public Function ResolveReceiver(IType type, Token name)
    {
        return receivers[type][name.ToString()];
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
                functions.Add(ResolveReceiver(type.GetPointerElementType(), func.name));
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
                    functions.Add(ResolveReceiver(type.GetPointerElementType(), func.name));
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