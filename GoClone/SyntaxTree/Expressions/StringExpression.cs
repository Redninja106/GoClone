using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Expressions;
internal class StringExpression : IExpression
{
    public Token value;
    public string stringValue;

    public LLVMValueRef Emit(EmitContext context, LLVMBuilderRef builder)
    {
        return LLVMValueRef.CreateConstArray(
            context.llvmCtx.Int32Type,
            stringValue.Select(c => LLVMValueRef.CreateConstInt(context.llvmCtx.Int32Type, (ulong)(int)c)).ToArray()
            );
    }

    public IType? GetResultType()
    {
        return new ArrayType { elementType = new PrimitiveType() { PrimitiveKind = TokenKind.Int }, intLength = stringValue.Length };
    }

    public IExpression Resolve(IScope scope)
    {
        stringValue = value.Value[1..^1].ToString();
        return this;
    }
}
