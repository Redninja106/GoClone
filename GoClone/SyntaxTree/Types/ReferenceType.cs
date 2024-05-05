using GoClone.CodeGeneration;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Types;
internal class ReferenceType : IType
{
    public required IType elementType;

    public unsafe LLVMTypeRef Emit(LLVMContextRef context)
    {
        if (!Valid())
        {
            throw new();
        }

        var opaquePtr = LLVM.PointerTypeInContext((LLVMOpaqueContext*)context.Handle, 0);
        return context.GetStructType([opaquePtr, opaquePtr], false);
    }

    private bool Valid()
    {
        if (elementType.GetEffectiveType() is InterfaceType interfaceType)
            return true;

        if (elementType.GetEffectiveType() is ArrayType arrayType && arrayType.length == null)
            return true;

        return false;
    }

    public bool Equals(IType? other)
    {
        return other is ReferenceType refType && elementType.Equals(refType.elementType);
    }

    public IType Resolve(IScope scope)
    {
        elementType = elementType.Resolve(scope);
        return this;
    }

    public override string ToString()
    {
        return elementType.ToString() + "&";
    }

    public override int GetHashCode()
    {
        // just do something so the hashcode isn't the same as the element type
        return HashCode.Combine(elementType.GetHashCode(), '&');
    }
}
