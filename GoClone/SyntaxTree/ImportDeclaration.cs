using GoClone.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree;
internal class ImportDeclaration : IDeclaration
{
    public Token moduleName;

    public static ImportDeclaration Parse(TokenReader reader)
    {
        reader.NextOrError(TokenKind.Import);
        var moduleName = reader.NextOrError(TokenKind.Identifier);
        return new ImportDeclaration { moduleName = moduleName };
    }

    public void Emit(ModuleScope scope)
    {
    }

    public void Register(ModuleScope scope)
    {
    }

    public void Resolve(ModuleScope scope)
    {
    }

    public void Verify(ModuleScope scope)
    {
    }
}
