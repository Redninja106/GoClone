using GoClone.CodeGeneration;
using GoClone.SyntaxTree.Types;
using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoClone.SyntaxTree.Expressions;
/*
 * struct TrueExpression {  }
 * 
 * func (TrueExpression expr) Emit(EmitContext context, LLVMBuilder* builder) -> LLVMValue* {
 *      return LLVMCreateConstInt(context.llvmCtx.Int1Type, 1)
 * }
 * 
 * func (TrueExpression expr) GetResultType() -> Type& {
 *      return { TokenKind.Bool } as PrimitiveType
 * }
 * 
 * func (TrueExpression expr) Resolve() -> Expression& {
 *      return expr
 * }
 * 
 */

internal class TrueExpression : IExpression
{
    public LLVMValueRef Emit(EmitContext context, LLVMBuilderRef builder)
    {
        return LLVMValueRef.CreateConstInt(context.llvmCtx.Int1Type, 1);
    }

    public IType GetResultType()
    {
        return new PrimitiveType { PrimitiveKind = TokenKind.Bool };
    }

    public IExpression Resolve(IScope scope)
    {
        return this;
    }
}
