using GoClone.CodeGeneration;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Types;
internal class PrimitiveType : IType
{
    public TokenKind PrimitiveKind { get; set; }

    public LLVMTypeRef Emit(LLVMContextRef context)
    {
        return PrimitiveKind switch
        {
            TokenKind.Int => context.Int32Type,
            TokenKind.Bool => context.Int1Type,
            _ => throw new(),
        };
    }

    public bool Equals(IType? other)
    {
        return other is PrimitiveType prim && this.PrimitiveKind == prim.PrimitiveKind;
    }

    public IType Resolve(IScope scope)
    {
        return this;
    }

    public override string ToString()
    {
        return PrimitiveKind.ToString().ToLower();
    }

    public override int GetHashCode()
    {
        return PrimitiveKind.GetHashCode();
    }
}
