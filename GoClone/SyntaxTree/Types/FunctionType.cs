using GoClone.CodeGeneration;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Types;
internal class FunctionType : IType
{
    public IType? returnType;
    public IType[] parameters;

    IType IType.Resolve(GoClone.CodeGeneration.IScope scope)
    {
        returnType = returnType.Resolve(scope);
        for (int i = 0; i < parameters.Length; i++)
        {
            parameters[i] = parameters[i].Resolve(scope);
        }
        return this;
    }

    public LLVMTypeRef Emit(LLVMContextRef context)
    {
        return LLVMTypeRef.CreateFunction(returnType?.Emit(context) ?? context.VoidType, parameters.Select(p => p.Emit(context)).ToArray());
    }

    public bool Equals(IType? other)
    {
        return other is FunctionType fn && returnType.Equals(fn.returnType) && parameters.SequenceEqual(fn.parameters);
    }
}
