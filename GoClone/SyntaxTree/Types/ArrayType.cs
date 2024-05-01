using GoClone.CodeGeneration;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Types;
internal class ArrayType : IType
{
    public IType elementType;
    public Token? length;

    IType IType.Resolve(GoClone.CodeGeneration.IScope scope)
    {
        elementType = elementType.Resolve(scope);
        return this;
    }

    public LLVMTypeRef Emit(LLVMContextRef context)
    {
        return LLVMTypeRef.CreateArray(elementType.Emit(context), uint.Parse(length.Value.ToString()));
    }

    public override string ToString()
    {
        return $"{elementType}[{length?.ToString() ?? ""}]";
    }

    public bool Equals(IType? other)
    {
        return other is ArrayType array && elementType.Equals(array.elementType) && length.Value.Value.SequenceEqual(length.Value.Value);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(elementType.GetHashCode(), length?.GetHashCode());
    }
}
