using GoClone.CodeGeneration;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Types;
internal class DeclaredType : IType
{
    public TypeDeclaration declaration;

    public LLVMTypeRef Emit(LLVMContextRef context)
    {
        return declaration.target.Emit(context);
    }

    public IType Resolve(IScope scope)
    {
        return this;
    }

    public override string ToString()
    {
        return declaration.name.ToString();
    }

    public bool Equals(IType? other)
    {
        return other is DeclaredType type && declaration == type.declaration;
    }

    public override int GetHashCode()
    {
        return declaration.GetHashCode();
    }
}
