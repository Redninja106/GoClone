using GoClone;
using GoClone.CodeGeneration;
using GoClone.SyntaxTree;
using System.Diagnostics;
using System.Numerics;

// ===NEW DESIGN===
// lex->parse->register->resolve->verify->transform->emit->emitbody
// register(scope,err): symbols should add themselves to the proper scope
// resolve(scope,err): identifier***s should resolve themselves into alternate types which explicitly reference declarations
// verify(err): main error checking here, ie type mismatch
// emit(ctx): symbols should generate their llvm values to referenced by codegen
// emitbody(ctx, blder): do codegen

string[] sources = [
    File.ReadAllText("code/libc.gc"),
    File.ReadAllText("code/core/string.gc"),
    // File.ReadAllText("code/core/list.gc"),
    // File.ReadAllText("code/project/lexer.gc"),
    File.ReadAllText("code/project/string_test.gc"),
    // File.ReadAllText("code/project/list_test.gc"),
    // File.ReadAllText("code/project/operator_overloads.gc"),
    // File.ReadAllText("code/project/interfaces.gc"),
];

List<Module> modules = new();
foreach (var source in sources)
{
    SourceReader reader = new(source);
    List<Token> tokens = [];
    while (true)
    {
        var token = Lexer.ReadToken(reader);

        if (token.kind != TokenKind.NewLine)
        {
            tokens.Add(token);
        }
        else if (tokens.Count > 0)
        {
            tokens[^1] = tokens[^1] with { hasLineTerminator = true };
        }

        if (token.kind == TokenKind.EndOfSource)
        {
            break;
        }
    }

    // foreach (var t in tokens)
    // {
    //     Console.WriteLine($"{t.Value.ToString(),-20} | {t.kind}");
    // }

    // Console.WriteLine(new string('=', 100));
    Module mod = Module.Parse(new([.. tokens]));
    // Console.WriteLine(mod);
    modules.Add(mod);
}


CodeGenerator generator = new();
var llvmMod = generator.EmitModule("main", modules.ToArray(), new ExceptionErrorHandler());
llvmMod.Dump();


llvmMod.TryVerify(LLVMSharp.Interop.LLVMVerifierFailureAction.LLVMPrintMessageAction, out string message);
bool success = string.IsNullOrEmpty(message);
Console.WriteLine(success ? "IL verification success" : "");
if (!success)
    return;
Console.WriteLine(new string('=', 100));

ModuleCompiler compiler = new();
compiler.LLVMCompile(llvmMod, "main.o");
bool linked = compiler.GccLink("main.o", "-o", "main.exe", "-lkernel32");
if (!linked)
    return;

var proc = Process.Start("main.exe");
proc.WaitForExit();