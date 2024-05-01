using GoClone.SyntaxTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone;
internal class Compiler
{
    private List<CompilationUnit> cus = [];

    public IEnumerable<CompilationUnit> CompilationUnits => cus;

    public CompilationUnit AddSource(string name, string source)
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

        var tokenArray = tokens.ToArray();
        return new CompilationUnit(source, tokenArray, Module.Parse(new(tokenArray)));
    }
}

class CompilationUnit(string source, Token[] tokens, Module module)
{
    public string source = source;
    public Token[] tokens = tokens;
    public Module module = module;

    public void DumpTokens()
    {
        foreach (var t in tokens)
        {
            Console.WriteLine($"{t.Value.ToString(),-20} | {t.kind}");
        }
    }
}