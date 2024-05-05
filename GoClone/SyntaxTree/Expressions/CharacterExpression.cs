using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Expressions;
internal class CharacterExpression : IExpression
{
    public Token character;

    public IExpression Resolve(IScope scope)
    {
        return this;
    }

    public LLVMValueRef Emit(EmitContext context, LLVMBuilderRef builder)
    {
        char c = character.Value.Trim('\'')[0];
        
        return LLVMValueRef.CreateConstInt(context.llvmCtx.Int32Type, c, false);
    }

    public IType GetResultType()
    {
        return new PrimitiveType { PrimitiveKind = TokenKind.Int };
    }

    public override string ToString()
    {
        return character.ToString();
    }
}
