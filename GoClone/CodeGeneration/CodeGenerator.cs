using GoClone.SyntaxTree;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Formats.Asn1.AsnWriter;

namespace GoClone.CodeGeneration;
internal class CodeGenerator
{
    public LLVMContextRef llvmContext = LLVMContextRef.Create();
    Dictionary<string, ModuleScope> modScopes = [];

    public LLVMModuleRef EmitModule(string name, Module[] modules, IErrorHandler errorHandler)
    {
        LLVMModuleRef llvmModule = llvmContext.CreateModuleWithName(name);

        // ModuleScope scope = new(llvmContext, llvmModule, errorHandler);

        foreach (var module in modules)
        {
            var scope = GetScope(module, llvmModule, errorHandler);
            foreach (var declaration in module.declarations)
            {
                declaration.Register(scope);
            }
        }

        RegisterImports(modules, llvmModule, errorHandler);

        foreach (var module in modules)
        {
            var scope = GetScope(module, llvmModule, errorHandler);
            foreach (var declaration in module.declarations)
            {
                declaration.Resolve(scope);
            }
        }

        foreach (var module in modules)
        {
            var scope = GetScope(module, llvmModule, errorHandler);
            foreach (var declaration in module.declarations)
            {
                declaration.Verify(scope);
            }
        }

        foreach (var module in modules)
        {
            var scope = GetScope(module, llvmModule, errorHandler);
            foreach (var declaration in module.declarations)
            {
                declaration.Emit(scope);
            }
        }

        foreach (var (_, scope) in modScopes)
        {
            scope.BuildVTables();
        }

        foreach (var module in modules)
        {
            var scope = GetScope(module, llvmModule, errorHandler);
            foreach (var fn in module.declarations.OfType<Function>())
            {
                fn.EmitBody(scope);
            }
        }

        return llvmModule;
    }

    private void RegisterImports(Module[] modules, LLVMModuleRef llvmModule, IErrorHandler errorHandler)
    {
        var coreMod = GetModuleByName(modules, "core");
        var coreScope = GetScope(coreMod, llvmModule, errorHandler);

        foreach (var (name, scope) in modScopes)
        {
            if (scope != coreScope)
                AddImports(scope, coreScope);
        }

        foreach (var module in modules)
        {
            var scope = GetScope(module, llvmModule, errorHandler);
            foreach (var import in module.declarations.OfType<ImportDeclaration>())
            {
                var importedModule = GetModuleByName(modules, import.moduleName.ToString());
                if (importedModule is null)
                {
                    throw new("module not found: " + import.moduleName.ToString());
                }
                var importScope = GetScope(importedModule, llvmModule, errorHandler);
                AddImports(scope, importScope);
            }
        }
    }

    private void AddImports(ModuleScope scope, ModuleScope importScope)
    {
        scope.importedScopes.Add(importScope);
    }

    private Module? GetModuleByName(Module[] modules, string name)
    {
        return modules.FirstOrDefault(mod => mod.declarations.First() is ModuleDeclaration decl && decl.moduleName.Value.SequenceEqual(name.AsSpan()));
    }

    private ModuleScope GetScope(Module module, LLVMModuleRef llvmModule, IErrorHandler errorHandler)
    {
        if (module.declarations.First() is not ModuleDeclaration decl)
            throw new();

        var name = decl.moduleName.ToString();
        if (modScopes.TryGetValue(name, out var scope))
        {
            return scope;
        }
        else
        {
            return modScopes[name] = new ModuleScope(this.llvmContext, llvmModule, errorHandler);
        }
    }
}
