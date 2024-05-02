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
// transform(err): syntax transformations here, ie interface{} into interface{}&
// emit(ctx): symbols should generate their llvm values to referenced by codegen
// emitbody(ctx, blder): do codegen

string source = File.ReadAllText("project/char.gc");
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

foreach (var t in tokens)
{
    Console.WriteLine($"{t.Value.ToString(),-20} | {t.kind}");
}

Console.WriteLine(new string('=', 100));
Module mod = Module.Parse(new([.. tokens]));
Console.WriteLine(mod);

Console.WriteLine(new string('=', 100));
CodeGenerator generator = new();
var llvmMod = generator.EmitModule("main", [mod]);
llvmMod.Dump();
Console.WriteLine(new string('=', 100));

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