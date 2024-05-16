using GoClone.CodeGeneration;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Types;
internal class PointerType : IType
{
    public IType elementType;

    public LLVMTypeRef Emit(LLVMContextRef context)
    {
        // var elem = elementType.Emit(context);
        unsafe 
        {
            LLVMOpaqueType* opaqueType = LLVM.PointerTypeInContext((LLVMOpaqueContext*)context.Handle, 0);
            return new LLVMTypeRef((nint)opaqueType);
        }
    }

    public bool Equals(IType? other)
    {
        return other is PointerType ptr && elementType.Equals(ptr.elementType);
    }

    public IType Resolve(IScope scope)
    {
        elementType = elementType.Resolve(scope);
        return this;
    }

    public override string ToString()
    {
        return $"{elementType}*";
    }
}
