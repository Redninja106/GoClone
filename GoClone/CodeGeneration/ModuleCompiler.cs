using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LLVMSharp;
using System.Diagnostics;

namespace GoClone.CodeGeneration;
internal class ModuleCompiler
{
    public unsafe void LLVMCompile(LLVMModuleRef module, string objectFileName)
    {
        LLVM.InitializeAllTargetInfos();
        LLVM.InitializeAllTargets();
        LLVM.InitializeAllTargetMCs();
        LLVM.InitializeAllAsmParsers();
        LLVM.InitializeAllAsmPrinters();

        var triple = LLVMTargetRef.DefaultTriple;
        var target = LLVMTargetRef.GetTargetFromTriple(triple);
        var targetMachine = target.CreateTargetMachine(triple, "generic", "", LLVMCodeGenOptLevel.LLVMCodeGenLevelNone, LLVMRelocMode.LLVMRelocDefault, LLVMCodeModel.LLVMCodeModelDefault);

        var layout = targetMachine.CreateTargetDataLayout();
        
        LLVM.SetModuleDataLayout(module, layout);
        module.Target = triple;

        targetMachine.EmitToFile(module, objectFileName, LLVMCodeGenFileType.LLVMObjectFile);
    }
    public bool LdLink(params string[] args)
    {
        var ld = Process.Start("ld", args);
        ld.WaitForExit();
        return ld.ExitCode <= 0;
    }
    public bool GccLink(params string[] args)
    {
        var ld = Process.Start("gcc", args);
        ld.WaitForExit();
        return ld.ExitCode <= 0;
    }
}
