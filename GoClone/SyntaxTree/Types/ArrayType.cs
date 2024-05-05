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
    public int? intLength;

    public IType Resolve(IScope scope)
    {
        if (length != null)
        {
            intLength = int.Parse(length.Value.Value);
        }
        elementType = elementType.Resolve(scope);
        return this;
    }

    public LLVMTypeRef Emit(LLVMContextRef context)
    {
        return LLVMTypeRef.CreateArray(elementType.Emit(context), (uint)(intLength ?? 0));
    }

    public override string ToString()
    {
        return $"{elementType}[{length?.ToString() ?? ""}]";
    }

    public bool Equals(IType? other)
    {
        if (other is ArrayType array)
        {
            if (elementType.Equals(array.elementType))
            {
                if (length is null && array.length is null)
                    return true;

                if (length!.Value.Value.SequenceEqual(length.Value.Value))
                    return true;
            }
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(elementType.GetHashCode(), intLength);
    }
}
