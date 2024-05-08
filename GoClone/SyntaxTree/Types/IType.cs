using GoClone.CodeGeneration;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Types;
internal interface IType : IEquatable<IType>
{
    IType Resolve(IScope scope);
    LLVMTypeRef Emit(LLVMContextRef context);

    IType GetBaseType()
    {
        if (this is ReferenceType reference)
        {
            return reference.elementType.GetBaseType();
        }

        if (this is PointerType ptr)
        {
            return ptr.elementType.GetBaseType();
        }

        if (this is DeclaredType declared)
        {
            return declared.declaration.target.GetBaseType();
        }

        return this;
    }

    IType GetEffectiveType()
    {
        if (this is DeclaredType declared)
        {
            return declared.declaration.target.GetEffectiveType();
        }

        return this;
    }

    IType GetPointerElementType()
    {
        if (this.GetEffectiveType() is PointerType ptr)
        {
            return ptr.elementType.GetPointerElementType();
        }

        return this;
    }
}
