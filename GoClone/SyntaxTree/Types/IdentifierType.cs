using GoClone.CodeGeneration;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Types;
internal class IdentifierType : IType
{
    public Token Identifier;

    public IType Resolve(IScope scope)
    {
        return new DeclaredType { declaration = (TypeDeclaration)scope.ResolveType(Identifier) };
    }

    public LLVMTypeRef Emit(LLVMContextRef context)
    {
        throw new Exception($"unresolved identifier {Identifier}");
    }

    public override string ToString()
    {
        return Identifier.ToString();
    }

    public bool Equals(IType? other)
    {
        throw new Exception($"unresolved identifier {Identifier}");
    }
}
