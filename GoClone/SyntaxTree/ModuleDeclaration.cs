﻿using GoClone.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree;
internal class ModuleDeclaration : IDeclaration
{
    public Token moduleName;

    public static ModuleDeclaration Parse(TokenReader reader)
    {
        reader.NextOrError(TokenKind.Module);
        var moduleName = reader.NextOrError(TokenKind.Identifier);
        return new ModuleDeclaration { moduleName = moduleName };
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
