using GoClone.SyntaxTree;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GoClone.CodeGeneration;
internal class CodeGenerator
{
    public LLVMContextRef llvmContext = LLVMContextRef.Create();

    public LLVMModuleRef EmitModule(string name, Module[] modules)
    {
        LLVMModuleRef llvmModule = llvmContext.CreateModuleWithName(name);

        ModuleScope scope = new(llvmContext, llvmModule);

        foreach (var module in modules)
        {
            foreach (var declaration in module.declarations)
            {
                declaration.Register(scope);
            }
        }

        foreach (var module in modules)
        {
            foreach (var declaration in module.declarations)
            {
                declaration.Resolve(scope);
            }
        }

        foreach (var module in modules)
        {
            foreach (var declaration in module.declarations)
            {
                declaration.Emit(scope);
            }
        }

        scope.BuildVTables();

        foreach (var module in modules)
        {
            foreach (var fn in module.declarations.OfType<Function>())
            {
                fn.EmitBody(scope);
            }
        }

        return llvmModule;
    }
}
