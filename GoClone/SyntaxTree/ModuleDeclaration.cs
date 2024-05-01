using GoClone.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree;
internal class ModuleDeclaration : IDeclaration
{
    public static ModuleDeclaration Parse(TokenReader reader)
    {
        throw new NotImplementedException();
    }

    public void Emit(ModuleScope scope)
    {
        throw new NotImplementedException();
    }

    public void Register(ModuleScope scope)
    {
        throw new NotImplementedException();
    }

    public void Resolve(ModuleScope scope)
    {
        throw new NotImplementedException();
    }
}
